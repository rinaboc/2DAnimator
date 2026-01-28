using System;

namespace Assets.Scripts.Utility.MVI
{
    public interface IModelContext
    {
        IRegistry<MeshData, MeshRegistry> Meshes { get; }
        IRegistry<Parameter, ParameterRegistry> Parameters { get; }
        IParamCurveRegistry ParamCurves { get; }
        IRegistry<ParamPoint, ParamPointRegistry> ParamPoints { get; }
        IKeyFrameRegistry KeyFrames { get; }
        IGeneralSettings GeneralSettings { get; }
    }
}