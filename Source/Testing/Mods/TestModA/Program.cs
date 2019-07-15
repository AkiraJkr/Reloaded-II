﻿using System;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using TestInterfaces;

namespace TestModA
{
    public class Program : IMod, IIdentifyMyself
    {
        public string MyId { get; set; } = "TestModA";

        /* Entry point. */
        public Action Disposing { get; }
        public void Start(IModLoaderV1 loader)
        {

        }

        /* Suspend/Unload */
        public void Suspend()
        {
            
        }

        public void Resume()
        {
            
        }

        public void Unload()
        {
            
        }

        public bool CanUnload() => true;
        public bool CanSuspend() => true;
    }
}