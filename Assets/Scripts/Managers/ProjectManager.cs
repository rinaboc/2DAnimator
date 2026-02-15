using System;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class ProjectManager : ManagerBase<ProjectManager>
{
    private IProjectService _projectService;
    [SerializeField] private AppInitializer _appInitializer;
    private IViewModel<OperationState, OperationState> _viewModel;

    void Start()
    {
        if (!_appInitializer.GetViewModel(out _viewModel))
        {
            Debug.LogError("Couldn't fetch viewModel");
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _projectService = new ProjectService();
    }


    public void SaveProject(string path, IModelContext context)
    {
        try
        {
            _projectService.Save(path, context);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save project: {ex.Message}");
        }
    }

    public void LoadProject(string path)
    {
        try
        {
            SaveData saveData = _projectService.Load(path);
            _viewModel?.Send(new InitializeProjectIntent(saveData));
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load project: {ex.StackTrace}");
            Debug.LogError(ex.Message);
        }
    }
}
