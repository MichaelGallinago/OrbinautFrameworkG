namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct HammerSpin : IAction
{
    public Player Player { private get; init; }
    
    public void Perform()
    {
        if (IsGrounded) return;
        
        if (Input.Down.Abc)
        {
            Charge();
            return;
        }
		
        switch (ActionValue)
        {
            case <= 0f: return;
			
            case >= PlayerConstants.MaxDropDashCharge:
                Animation = Animations.Spin;
                Action = Actions.HammerSpinCancel;
                break;
        }

        ActionValue = 0f;
    }

    private void Charge()
    {
        ActionValue += Scene.Local.ProcessSpeed;
        if (ActionValue >= MaxDropDashCharge)
        {
            AudioPlayer.Sound.Play(SoundStorage.Charge3);
        }

        IsAirLock = false;
    }
}