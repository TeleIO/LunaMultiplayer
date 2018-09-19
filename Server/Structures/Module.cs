﻿using LunaConfigNode;

namespace Server.Structures
{
    public class Module
    {
        public MixedCollection<string, string> Fields;
        public ConfigNode Events;
        public ConfigNode Actions;
        public ConfigNode Upgradesapplied;

        public Module(ConfigNode cfgNode)
        {
            Fields = new MixedCollection<string, string>(cfgNode.GetAllValues());

            Events = cfgNode.GetNodes("EVENTS")[0];
            Actions = cfgNode.GetNodes("ACTIONS")[0];
            Upgradesapplied = cfgNode.GetNodes("UPGRADESAPPLIED")[0];
        }
    }
}