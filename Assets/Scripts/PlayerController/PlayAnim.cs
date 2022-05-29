using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnim : MonoBehaviour
{
    Animator hitAnim;
    bool isHitting;

    private void Awake()
    {
        hitAnim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
            isHitting = true;
        else
            isHitting = false;

        hitAnim.SetBool("isHitting", isHitting);
    }
}
