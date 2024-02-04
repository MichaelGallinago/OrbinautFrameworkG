using OrbinautFramework3.Audio.Player;

namespace OrbinautFramework3.Stages.TSZ;

public partial class StageTsz : Framework.CommonStage
{
    public override void _Ready()
    {
        //TODO: StageSetup in StageTsz
        base._Ready();
        Music = MusicStorage.TestStage;
    }
}
