﻿using System;

namespace EndlessHeresy.Gameplay.Services.Input
{
    public interface IInputService
    {
        public event Action<bool> OnLockChanged;
        public bool IsLocked { get; }
        public void Lock();
        public void Unlock();
    }
}