using System;

public interface ISelectable
{
    void SetSelected(bool isSelected);

    void OnSelect(Guid id);

    void OnDeselect();
}
