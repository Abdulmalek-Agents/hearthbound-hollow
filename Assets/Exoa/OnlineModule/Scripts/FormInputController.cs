using Exoa.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Exoa.Designer
{
    public class FormInputController : MonoBehaviour
    {

        public TMP_InputField[] fields;
        public Button submitButton;

        void Start()
        {
            foreach (TMP_InputField input in fields)
            {
                input.onEndEdit.AddListener(OnInputEndEdit);
            }
        }


        private void OnInputEndEdit(string arg0)
        {
            if (arg0 == "")
                return;

            //print("OnInputEndEdit " + arg0);
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].text == arg0)
                {
                    if (fields.Length > i + 1)
                    {
                        fields[i + 1].ActivateInputField();
                        fields[i + 1].Select();
                    }
                    else
                    {
                        submitButton.OnSubmit(null);
                    }
                }
            }
        }

        void Update()
        {
            if (BaseTouchInput.GetKeyWentDown(KeyCode.Tab))
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    //print("fields[i].isFocused " + fields[i].isFocused);
                    if (fields[i].isFocused)
                    {
                        if (fields.Length > i + 1)
                        {
                            //print("ActivateInputField " + (i + 1));
                            fields[i + 1].ActivateInputField();
                            fields[i + 1].Select();
                        }
                    }
                }
            }
        }
    }
}
