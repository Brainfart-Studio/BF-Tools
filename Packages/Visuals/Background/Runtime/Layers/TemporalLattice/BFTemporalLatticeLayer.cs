using System.Collections.Generic;
using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFTemporalLatticeLayer
        : IBFBackgroundLayer
    {
        private static readonly string[] LogTags =
        {
            "Background",
            "TemporalLattice"
        };

        private sealed class VisualCell
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public BFTemporalLatticeCell Simulation;
        }

        private readonly BFTemporalLatticeLayerConfig config;

        private readonly List<VisualCell> visualCells =
            new List<VisualCell>();

        private BFTemporalLatticeCell[,] cells;
        private BFTemporalLatticeCellState[,] neighbors;

        private Transform root;
        private Sprite cellSprite;

        private int columns;
        private int rows;
        private int historyLength;

        private float elapsed;
        private float nextImpulseTime;

        private Vector2 impulsePosition;

        public BFTemporalLatticeLayer(
            BFTemporalLatticeLayerConfig config)
        {
            this.config = config;
        }

        public void Init(
            Transform parent,
            int sortingOrder)
        {
            GameObject rootObject =
                new GameObject(
                    "BFTemporalLatticeLayer"
                );

            rootObject.transform.SetParent(
                parent,
                false
            );

            rootObject.layer =
                BFBackgroundStackManager.BackgroundLayer;

            root =
                rootObject.transform;

            cellSprite =
                CreateCellSprite();

            Rebuild(
                sortingOrder
            );

            nextImpulseTime =
                elapsed +
                config.ImpulseInterval;

            BFLogger.Info(
                LogTags,
                "BFTemporalLatticeLayer: initialized."
            );
        }

        public void Tick(float dt)
        {
            if (root == null)
                return;

            elapsed += dt;

            if (NeedsRebuild())
                Rebuild(0);

            UpdateImpulse();
            CalculateNeighbors();
            Simulate(dt);
            PushHistory();
            UpdateVisuals();
        }

        private bool NeedsRebuild()
        {
            return
                columns != config.Columns ||
                rows != config.Rows ||
                historyLength != config.HistoryLength;
        }

        private void Rebuild(
            int sortingOrder)
        {
            ClearVisualCells();

            columns =
                Mathf.Max(
                    2,
                    config.Columns
                );

            rows =
                Mathf.Max(
                    2,
                    config.Rows
                );

            historyLength =
                Mathf.Max(
                    1,
                    config.HistoryLength
                );

            cells =
                new BFTemporalLatticeCell[
                    columns,
                    rows
                ];

            neighbors =
                new BFTemporalLatticeCellState[
                    columns,
                    rows
                ];

            for (int y = 0; y < rows; y++)
            {
                float ny =
                    y /
                    (float)(rows - 1);

                for (int x = 0; x < columns; x++)
                {
                    float nx =
                        x /
                        (float)(columns - 1);

                    BFTemporalLatticeCell simulation =
                        new BFTemporalLatticeCell(
                            config,
                            nx,
                            ny
                        );

                    cells[x, y] =
                        simulation;

                    GameObject cellObject =
                        new GameObject(
                            "Cell_" +
                            x +
                            "_" +
                            y
                        );

                    cellObject.transform.SetParent(
                        root,
                        false
                    );

                    cellObject.layer =
                        BFBackgroundStackManager.BackgroundLayer;

                    SpriteRenderer renderer =
                        cellObject.AddComponent<SpriteRenderer>();

                    renderer.sprite =
                        cellSprite;

                    renderer.sortingOrder =
                        sortingOrder;

                    visualCells.Add(
                        new VisualCell
                        {
                            GameObject = cellObject,
                            Transform =
                                cellObject.transform,
                            Renderer = renderer,
                            Simulation = simulation
                        }
                    );
                }
            }

            nextImpulseTime =
                elapsed +
                config.ImpulseInterval;

            UpdateVisuals();
        }

        private Sprite CreateCellSprite()
        {
            Texture2D texture =
                new Texture2D(
                    1,
                    1,
                    TextureFormat.RGBA32,
                    false
                );

            texture.name =
                "BFTemporalLatticeCellTexture";

            texture.SetPixel(
                0,
                0,
                Color.white
            );

            texture.Apply();

            texture.filterMode =
                FilterMode.Bilinear;

            texture.wrapMode =
                TextureWrapMode.Clamp;

            return Sprite.Create(
                texture,
                new Rect(
                    0,
                    0,
                    1,
                    1
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                1f
            );
        }

        private void UpdateImpulse()
        {
            if (config.ImpulseInterval <= 0f)
                return;

            while (elapsed >= nextImpulseTime)
            {
                float seedX =
                    Mathf.Sin(
                        nextImpulseTime * 2.173f +
                        17.37f
                    ) *
                    0.5f +
                    0.5f;

                float seedY =
                    Mathf.Sin(
                        nextImpulseTime * 1.713f +
                        47.11f
                    ) *
                    0.5f +
                    0.5f;

                impulsePosition =
                    new Vector2(
                        Mathf.Lerp(
                            0.5f,
                            seedX,
                            config.ImpulseVariation
                        ),
                        Mathf.Lerp(
                            0.5f,
                            seedY,
                            config.ImpulseVariation
                        )
                    );

                nextImpulseTime +=
                    config.ImpulseInterval;
            }
        }

        private void CalculateNeighbors()
        {
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    float energy = 0f;
                    Vector2 offset = Vector2.zero;
                    Vector2 velocity = Vector2.zero;
                    int count = 0;

                    AddNeighbor(
                        x - 1,
                        y,
                        ref energy,
                        ref offset,
                        ref velocity,
                        ref count
                    );

                    AddNeighbor(
                        x + 1,
                        y,
                        ref energy,
                        ref offset,
                        ref velocity,
                        ref count
                    );

                    AddNeighbor(
                        x,
                        y - 1,
                        ref energy,
                        ref offset,
                        ref velocity,
                        ref count
                    );

                    AddNeighbor(
                        x,
                        y + 1,
                        ref energy,
                        ref offset,
                        ref velocity,
                        ref count
                    );

                    if (count > 0)
                    {
                        float inverse =
                            1f / count;

                        energy *= inverse;
                        offset *= inverse;
                        velocity *= inverse;
                    }

                    neighbors[x, y] =
                        new BFTemporalLatticeCellState
                        {
                            energy = energy,
                            offset = offset,
                            velocity = velocity
                        };
                }
            }
        }

        private void AddNeighbor(
            int x,
            int y,
            ref float energy,
            ref Vector2 offset,
            ref Vector2 velocity,
            ref int count)
        {
            if (
                x < 0 ||
                x >= columns ||
                y < 0 ||
                y >= rows
            )
            {
                return;
            }

            BFTemporalLatticeCellState state =
                cells[x, y].Current;

            energy += state.energy;
            offset += state.offset;
            velocity += state.velocity;

            count++;
        }

        private void Simulate(float dt)
        {
            float radius =
                Mathf.Max(
                    0.001f,
                    config.ImpulseRadius
                );

            for (int y = 0; y < rows; y++)
            {
                float ny =
                    y /
                    (float)(rows - 1);

                for (int x = 0; x < columns; x++)
                {
                    float nx =
                        x /
                        (float)(columns - 1);

                    float impulse =
                        GetImpulse(
                            nx,
                            ny,
                            radius
                        );

                    float waveA =
                        Mathf.Sin(
                            nx * 7.1f +
                            elapsed * 0.8f +
                            ny * 2.3f
                        );

                    float waveB =
                        Mathf.Sin(
                            ny * 9.3f -
                            elapsed * 0.61f +
                            nx * 3.7f
                        );

                    float wave =
                        Mathf.Max(
                            0f,
                            (
                                waveA +
                                waveB
                            ) *
                            0.25f +
                            0.25f
                        );

                    cells[x, y].Tick(
                        dt,
                        elapsed,
                        neighbors[x, y].energy,
                        neighbors[x, y].offset,
                        neighbors[x, y].velocity,
                        impulse,
                        wave
                    );
                }
            }
        }

        private float GetImpulse(
            float x,
            float y,
            float radius)
        {
            float dx =
                x -
                impulsePosition.x;

            float dy =
                y -
                impulsePosition.y;

            float distance =
                Mathf.Sqrt(
                    dx * dx +
                    dy * dy
                );

            if (distance >= radius)
                return 0f;

            float t =
                1f -
                distance / radius;

            return t * t;
        }

        private void PushHistory()
        {
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                    cells[x, y].PushHistory();
            }
        }

        private void UpdateVisuals()
        {
            float cellWidth =
                Screen.width /
                (float)columns;

            float cellHeight =
                Screen.height /
                (float)rows;

            float width =
                Mathf.Max(
                    1f,
                    cellWidth *
                    (1f - config.CellGap)
                );

            float height =
                Mathf.Max(
                    1f,
                    cellHeight *
                    (1f - config.CellGap)
                );

            int index = 0;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    VisualCell visual =
                        visualCells[index++];

                    BFTemporalLatticeCellState state =
                        visual.Simulation.Current;

                    float energy =
                        Mathf.Clamp01(
                            state.energy
                        );

                    float velocity =
                        Mathf.Clamp01(
                            state.velocity.magnitude /
                            Mathf.Max(
                                0.001f,
                                config.MaxVelocity
                            )
                        );

                    float scale =
                        1f +
                        energy *
                        config.EnergyScale +
                        velocity *
                        config.VelocityScale;

                    float px =
                        x *
                        cellWidth +
                        cellWidth *
                        0.5f;

                    float py =
                        y *
                        cellHeight +
                        cellHeight *
                        0.5f;

                    visual.Transform.localPosition =
                        new Vector3(
                            px + state.offset.x,
                            py + state.offset.y,
                            0f
                        );

                    visual.Transform.localScale =
                        new Vector3(
                            width * scale,
                            height * scale,
                            1f
                        );

                    float velocityAngle =
                        Mathf.Atan2(
                            state.velocity.y,
                            state.velocity.x
                        ) *
                        Mathf.Rad2Deg;

                    float rotation =
                        velocityAngle *
                        velocity *
                        config.RotationAmount /
                        90f;

                    visual.Transform.localRotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            rotation
                        );

                    visual.Renderer.color =
                        state.color;
                }
            }
        }

        private void ClearVisualCells()
        {
            for (int i = 0; i < visualCells.Count; i++)
            {
                if (
                    visualCells[i] != null &&
                    visualCells[i].GameObject != null
                )
                {
                    if (Application.isPlaying)
                    {
                        Object.Destroy(
                            visualCells[i].GameObject
                        );
                    }
                    else
                    {
                        Object.DestroyImmediate(
                            visualCells[i].GameObject
                        );
                    }
                }
            }

            visualCells.Clear();
        }

        public void Cleanup()
        {
            ClearVisualCells();

            if (cellSprite != null)
            {
                Texture2D texture =
                    cellSprite.texture;

                if (Application.isPlaying)
                {
                    Object.Destroy(cellSprite);

                    if (texture != null)
                        Object.Destroy(texture);
                }
                else
                {
                    Object.DestroyImmediate(cellSprite);

                    if (texture != null)
                        Object.DestroyImmediate(texture);
                }
            }

            cellSprite = null;

            if (root != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(root.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(root.gameObject);
                }
            }

            root = null;
            cells = null;
            neighbors = null;

            BFLogger.Info(
                LogTags,
                "BFTemporalLatticeLayer: cleaned up."
            );
        }
    }
}