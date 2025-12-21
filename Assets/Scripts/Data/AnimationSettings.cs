using System;

[Serializable]
public class AnimationSettings
{
    public int maxFrames;
    public int framePerSec;

    public AnimationSettings(int maxFrames, int framePerSec)
    {
        this.maxFrames = maxFrames;
        this.framePerSec = framePerSec;
    }
}