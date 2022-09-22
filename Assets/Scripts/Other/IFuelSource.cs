using System;

namespace RobotFighter.Core
{
    internal interface IFuelSource
    {
        delegate void ConsumedEventHandler(object sender, int fuel);
        delegate void EliminatedEventHandler(object sender);

        event ConsumedEventHandler Consumed;
        event EliminatedEventHandler BeingDestroyed;
    }
}