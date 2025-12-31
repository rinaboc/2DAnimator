using System;
using Assets.Scripts.Utility.MVI;

public class LayerCommandHandler : ICommandHandler
{
    public void Execute(IIntent intent, object state, IModelContext context)
    {
        switch (intent)
        {
            case ChangeLayerNameIntent change:
                ExecuteChangeLayerName(change, state, context);
                break;
            case CreateMeshLayerIntent create:
                ExecuteCreateMeshLayer(create, state, context);
                break;
            case SelectLayerIntent select:
                ExecuteSelectLayer(select, state, context);
                break;
            case MoveLayerUpIntent _:
                ExecuteMoveLayerUp(state, context);
                break;
            case MoveLayerDownIntent _:
                ExecuteMoveLayerDown(state, context);
                break;
            case DeleteLayerIntent _:
                ExecuteDeleteLayer(state, context);
                break;
        }
    }

    private void ExecuteDeleteLayer(object state, IModelContext context)
    {
        LayerManager.Instance.DeleteUILayer(context.GeneralSettings.SelectedMeshID);
    }

    private void ExecuteMoveLayerDown(object state, IModelContext context)
    {
        LayerManager.Instance.MoveLayerDown(context.GeneralSettings.SelectedMeshID);
    }

    private void ExecuteMoveLayerUp(object state, IModelContext context)
    {
        LayerManager.Instance.MoveLayerUp(context.GeneralSettings.SelectedMeshID);
    }

    private void ExecuteSelectLayer(SelectLayerIntent select, object state, IModelContext context)
    {
        context.GeneralSettings.SelectedMeshID = select.LayerID;
    }

    private void ExecuteCreateMeshLayer(CreateMeshLayerIntent create, object state, IModelContext context)
    {
        LayerManager.Instance.CreateUIArtLayer(create.ID);
    }

    private void ExecuteChangeLayerName(ChangeLayerNameIntent change, object state, IModelContext context)
    {
        if (context.Meshes.TryGet(change.LayerID, out MeshData meshData))
        {
            meshData.name = change.NewName;
        }
    }
}
