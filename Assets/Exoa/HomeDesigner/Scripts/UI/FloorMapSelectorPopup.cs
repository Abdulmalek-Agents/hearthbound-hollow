using System;
using Exoa.Designer;
using Exoa.Events;
using UnityEngine.UI;

public class FloorMapSelectorPopup : BaseStaticPopup
{

    /// <summary>Unsubscribes from the event handler when the object is destroyed.</summary>
    void OnDestroy()
    {
        GameEditorEvents.OnRequestButtonAction -= OnRequestButtonAction;
    }

    override protected void Awake()
    {
        base.Awake();

        GameEditorEvents.OnRequestButtonAction += OnRequestButtonAction;
    }

    private void OnRequestButtonAction(GameEditorEvents.Action action, bool active)
    {
        if (action == GameEditorEvents.Action.NewProject)
        {
            Show();
        }
    }


}
