﻿using System.Collections.Generic;
using System.IO;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon.Enums;
using LunaCommon.Message.Data.CraftLibrary;
using UnityEngine;

namespace LunaClient.Systems.CraftLibrary
{
    public class CraftLibraryEvents : SubSystem<CraftLibrarySystem>
    {
        private ScreenMessage CraftUploadMessage { get; set; }
        private bool DisplayCraftUploadingMessage { get; set; }
        private float LastCraftMessageCheck { get; set; }
        private float CraftMessageCheckInterval { get; } = 0.2f;

        public void HandleCraftLibraryEvents()
        {
            CraftChangeEntry craftChangeEntry;
            while (System.CraftAddQueue.TryDequeue(out craftChangeEntry))
                AddCraftEntry(craftChangeEntry.PlayerName, craftChangeEntry.CraftType, craftChangeEntry.CraftName);

            while (System.CraftDeleteQueue.TryDequeue(out craftChangeEntry))
                DeleteCraftEntry(craftChangeEntry.PlayerName, craftChangeEntry.CraftType, craftChangeEntry.CraftName);

            CraftResponseEntry cre;
            while (System.CraftResponseQueue.TryDequeue(out cre))
                SaveCraftFile(cre.CraftType, cre.CraftName, cre.CraftData);

            if (System.UploadCraftName != null)
            {
                UploadCraftFile(System.UploadCraftType, System.UploadCraftName);
                System.UploadCraftName = null;
                System.UploadCraftType = CraftType.VAB;
            }

            if (System.DownloadCraftName != null)
            {
                DownloadCraftFile(System.SelectedPlayer, System.DownloadCraftType, System.DownloadCraftName);
                System.DownloadCraftName = null;
                System.DownloadCraftType = CraftType.VAB;
            }

            if (System.DeleteCraftName != null)
            {
                DeleteCraftEntry(SettingsSystem.CurrentSettings.PlayerName, System.DeleteCraftType, System.DeleteCraftName);
                System.MessageSender.SendMessage(new CraftLibraryDeleteMsgData
                {
                    PlayerName = SettingsSystem.CurrentSettings.PlayerName,
                    CraftType = System.DeleteCraftType,
                    CraftName = System.DeleteCraftName
                });
                System.DeleteCraftName = null;
                System.DeleteCraftType = CraftType.VAB;
            }

            if (DisplayCraftUploadingMessage &&
                (Time.realtimeSinceStartup - LastCraftMessageCheck > CraftMessageCheckInterval))
            {
                LastCraftMessageCheck = Time.realtimeSinceStartup;
                if (CraftUploadMessage != null)
                    CraftUploadMessage.duration = 0f;
                DisplayCraftUploadingMessage = false;
                CraftUploadMessage = ScreenMessages.PostScreenMessage("Craft uploaded!", 2f,
                    ScreenMessageStyle.UPPER_CENTER);
            }
        }

        private void UploadCraftFile(CraftType type, string name)
        {
            var uploadPath = "";
            switch (System.UploadCraftType)
            {
                case CraftType.VAB:
                    uploadPath = System.VabPath;
                    break;
                case CraftType.SPH:
                    uploadPath = System.SphPath;
                    break;
                case CraftType.SUBASSEMBLY:
                    uploadPath = System.SubassemblyPath;
                    break;
            }
            var filePath = CommonUtil.CombinePaths(uploadPath, name + ".craft");
            if (File.Exists(filePath))
            {
                var fileData = File.ReadAllBytes(filePath);

                System.MessageSender.SendMessage(new CraftLibraryUploadMsgData
                {
                    PlayerName = SettingsSystem.CurrentSettings.PlayerName,
                    UploadType = type,
                    UploadName = name,
                    CraftData = fileData
                });
                AddCraftEntry(SettingsSystem.CurrentSettings.PlayerName, System.UploadCraftType, System.UploadCraftName);
                DisplayCraftUploadingMessage = true;
            }
            else
            {
                LunaLog.Debug("Cannot upload file, " + filePath + " does not exist!");
            }
        }

        private static void DownloadCraftFile(string playerName, CraftType craftType, string craftName)
        {
            System.MessageSender.SendMessage(new CraftLibraryRequestMsgData
            {
                PlayerName = SettingsSystem.CurrentSettings.PlayerName,
                RequestedType = craftType,
                CraftOwner = playerName,
                RequestedName = craftName
            });
        }

        private static void AddCraftEntry(string playerName, CraftType craftType, string craftName)
        {
            if (!System.PlayersWithCrafts.Contains(playerName))
                System.PlayersWithCrafts.Add(playerName);
            if (!System.PlayerList.ContainsKey(playerName))
                System.PlayerList.Add(playerName, new Dictionary<CraftType, List<string>>());
            if (!System.PlayerList[playerName].ContainsKey(craftType))
                System.PlayerList[playerName].Add(craftType, new List<string>());
            if (!System.PlayerList[playerName][craftType].Contains(craftName))
            {
                LunaLog.Debug("Adding " + craftName + ", type: " + craftType + " from " + playerName);
                System.PlayerList[playerName][craftType].Add(craftName);
            }
        }

        private static void DeleteCraftEntry(string playerName, CraftType craftType, string craftName)
        {
            if (System.PlayerList.ContainsKey(playerName) &&
                System.PlayerList[playerName].ContainsKey(craftType) &&
                System.PlayerList[playerName][craftType].Contains(craftName))
            {
                System.PlayerList[playerName][craftType].Remove(craftName);
                if (System.PlayerList[playerName][craftType].Count == 0)
                    System.PlayerList[playerName].Remove(craftType);
                if ((System.PlayerList[playerName].Count == 0) && (playerName != SettingsSystem.CurrentSettings.PlayerName))
                {
                    System.PlayerList.Remove(playerName);
                    if (System.PlayersWithCrafts.Contains(playerName))
                        System.PlayersWithCrafts.Remove(playerName);
                }
            }
        }

        private static void SaveCraftFile(CraftType craftType, string craftName, byte[] craftData)
        {
            var savePath = "";
            switch (craftType)
            {
                case CraftType.VAB:
                    savePath = System.VabPath;
                    break;
                case CraftType.SPH:
                    savePath = System.SphPath;
                    break;
                case CraftType.SUBASSEMBLY:
                    savePath = System.SubassemblyPath;
                    break;
            }

            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            var craftFile = CommonUtil.CombinePaths(savePath, craftName + ".craft");
            File.WriteAllBytes(craftFile, craftData);
            ScreenMessages.PostScreenMessage("Craft " + craftName + " saved!", 5f, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}