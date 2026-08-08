using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.ProjectSetup.Editor;

namespace BFTools.EditorTools.ProjectSetup.Editor
{
    public static class BFNewProjectSetup
    {
        private const string LogTag = "ProjectSetup";

        [MenuItem("BF Tools/New Project Setup")]
        private static void Run()
        {
            List<IBFProjectSetupStep> steps = TypeCache.GetTypesDerivedFrom<IBFProjectSetupStep>()
                .Where(type => !type.IsAbstract && !type.IsInterface)
                .Select(type => (IBFProjectSetupStep)Activator.CreateInstance(type))
                .OrderBy(step => step.Order)
                .ToList();

            List<string> results = new List<string>();
            foreach (IBFProjectSetupStep step in steps)
            {
                string result = step.Run();
                if (!string.IsNullOrEmpty(result))
                    results.Add(result);
            }

            GameObject[] contributedPrefabs = steps
                .OfType<IBFSystemPrefabContributor>()
                .Select(contributor => contributor.SystemPrefab)
                .Where(prefab => prefab != null)
                .ToArray();

            foreach (IBFSystemPrefabConsumer consumer in steps.OfType<IBFSystemPrefabConsumer>())
                consumer.AssignSystemPrefabs(contributedPrefabs);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string summary = results.Count > 0 ? string.Join("\n", results) : "Nothing to set up.";
            BFLogger.Info(LogTag, $"New project setup complete.\n{summary}");
            EditorUtility.DisplayDialog("BF Tools", $"Project setup complete.\n\n{summary}", "OK");
        }
    }
}