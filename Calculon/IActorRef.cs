﻿using System;

namespace Droog.Calculon {
    public interface IActorRef {
        string Name { get; }
        Type Type { get; }
    }
}