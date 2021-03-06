﻿using System;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Screenshot;
using LmpCommon.Message.Interface;
using LmpCommon.Time;
using UniLinq;

namespace LmpClient.Systems.Screenshot
{
    public class ScreenshotMessageSender : SubSystem<ScreenshotSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ScreenshotCliMsg>(msg)));
        }

        public void SendScreenshot(byte[] data)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ScreenshotDataMsgData>();

            msgData.Screenshot.DateTaken = LunaNetworkTime.UtcNow.ToBinary();
            msgData.Screenshot.NumBytes = data.Length;

            if(msgData.Screenshot.Data.Length < msgData.Screenshot.NumBytes)
                msgData.Screenshot.Data = new byte[msgData.Screenshot.NumBytes];

            Array.Copy(data, msgData.Screenshot.Data, msgData.Screenshot.NumBytes);

            SendMessage(msgData);
        }

        public void RequestFolders()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ScreenshotFoldersRequestMsgData>();
            SendMessage(msgData);
        }

        public void RequestMiniatures(string folderName)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ScreenshotListRequestMsgData>();
            msgData.FolderName = folderName;

            if (System.MiniatureImages.TryGetValue(folderName, out var miniatureDictionary))
            {
                msgData.AlreadyOwnedPhotoIds = miniatureDictionary.Keys.ToArray();
                msgData.NumAlreadyOwnedPhotoIds = miniatureDictionary.Count;
            }
            else
            {
                msgData.NumAlreadyOwnedPhotoIds = 0;
            }

            SendMessage(msgData);
        }

        public void RequestImage(string folderName, long dateTaken)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ScreenshotDownloadRequestMsgData>();
            msgData.FolderName = folderName;
            msgData.DateTaken = dateTaken;

            SendMessage(msgData);
        }
    }
}
