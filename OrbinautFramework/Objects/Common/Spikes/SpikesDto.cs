using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Common.Spikes;

public readonly record struct SpikesDto(bool IsFlipped, Constants.CollisionSensor Sensor, int RetractDistance);
