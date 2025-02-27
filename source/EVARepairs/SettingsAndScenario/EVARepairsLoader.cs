﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP.Localization;

namespace EVARepairs.SettingsAndScenario
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    sealed class EVARepairsLoader : MonoBehaviour
    {
        class RepairModulesLoader : LoadingSystem
        {
            const string kModuleNameToAdd = "ModuleEVARepairs";

            public override bool IsReady()
            {
                return true;
            }

            public override void StartLoad()
            {
                int count = PartLoader.LoadedPartsList.Count;
                AvailablePart availablePart;
                PartModule partModule;

                // Find the baseline config node.
                ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes("EVAREPAIRS_BASELINE_CONFIG");
                ConfigNode baselineConfig;
                if (nodes == null || nodes.Length == 0)
                    return;
                baselineConfig = nodes[0];
                if (!baselineConfig.HasNode("MODULE"))
                    return;
                baselineConfig = baselineConfig.GetNode("MODULE");
                if (!baselineConfig.HasValue("name") || baselineConfig.GetValue("name") != kModuleNameToAdd)
                    return;

                // Now, go through each part and see if it needs to have a ModuleEVARepairs.
                for (int index = 0; index < count; index++)
                {
                    // Get the available part
                    availablePart = PartLoader.LoadedPartsList[index];

                    // If the part already has ModuleEVARepairs then we're done.
                    if (availablePart.partPrefab.HasModuleImplementing<ModuleEVARepairs>())
                        continue;

                    // If the part doesn't have a ModuleGenerator, ModuleEngines, or BaseConverter, then we're done.
                    if (!availablePart.partPrefab.HasModuleImplementing<ModuleGenerator>() &&
                        !availablePart.partPrefab.HasModuleImplementing<ModuleEngines>() &&
                        !availablePart.partPrefab.HasModuleImplementing<BaseConverter>()
                        )
                        continue;

                    // Add the module and load the config.
                    partModule = availablePart.partPrefab.AddModule(kModuleNameToAdd, true);
                    if (partModule != null)
                        partModule.Load(baselineConfig);

                    // Add module info to the prefab.
                    if (partModule is IModuleInfo)
                    {
                        IModuleInfo info = partModule as IModuleInfo;
                        AvailablePart.ModuleInfo moduleInfo = new AvailablePart.ModuleInfo();

                        moduleInfo.onDrawWidget = info.GetDrawModulePanelCallback();
                        moduleInfo.moduleName = info.GetModuleTitle();
                        moduleInfo.moduleDisplayName = partModule.GetModuleDisplayName();
                        moduleInfo.info = info.GetInfo().Trim();

                        availablePart.moduleInfos.Add(moduleInfo);
                        availablePart.moduleInfos.Sort(((ap1, ap2) => ap1.moduleName.CompareTo(ap2.moduleName)));
                    }
                }
            }

            public override string ProgressTitle()
            {
                return Localizer.Format("#LOC_EVAREPAIRS_partLoaderTitle");
            }
        }

        #region Overrides
        public void Awake()
        {
            List<LoadingSystem> loaders = LoadingScreen.Instance.loaders;
            if (loaders != null)
            {
                int count = loaders.Count;
                for (int index = 0; index < count; index++)
                {
                    if (loaders[index] is PartLoader)
                    {
                        GameObject gameObject = new GameObject();
                        RepairModulesLoader modulesLoader = gameObject.AddComponent<RepairModulesLoader>();
                        loaders.Insert(index + 1, modulesLoader);
                        break;
                    }
                }
            }
        }

        #endregion
    }
}