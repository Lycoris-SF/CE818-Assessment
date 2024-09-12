// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Author: Lycoris

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyIndicator : MonoBehaviour
{
    public GameObject enemy;
    public GameObject EnemyPointerPanel;
    public RectTransform EnemyPointerOutside;
    public RectTransform EnemyPointerInside;
    public Canvas CanvasPointer;
    public int margin = 20;
    public List<Text> Debug_text;

    private bool isEnemyOutside;
    private bool isEnemyAtback;

    private void Start()
    {

    }
    void Update()
    {
        if (Camera.main)
        {
            if (enemy)
            {
                EnemyPointerPanel.SetActive(true);
                Vector3 screenPos = Camera.main.WorldToViewportPoint(enemy.transform.position);
                isEnemyOutside = screenPos.x < 0 || screenPos.x > 1 || screenPos.y < 0 || screenPos.y > 1;
                isEnemyAtback = screenPos.z < 0;

                if (isEnemyOutside || isEnemyAtback)
                {
                    UpdateOutsidePointerPosition(screenPos);
                    EnemyPointerOutside.gameObject.SetActive(true);
                    EnemyPointerInside.gameObject.SetActive(false);
                }
                else
                {
                    UpdatePointerInsidePosition(screenPos);
                    EnemyPointerOutside.gameObject.SetActive(false);
                    EnemyPointerInside.gameObject.SetActive(true);
                    if (Mathf.Abs(screenPos.z) < 1000)
                    {

                    }
                }
            }
        }
    }
    private void FixedUpdate()
    {
        if (!enemy)
        {
            EnemyPointerPanel.SetActive(false);
        }
    }

    void UpdateOutsidePointerPosition(Vector3 screenPos)
    {
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasPointer.GetComponent<RectTransform>(), new Vector2(screenPos.x * Screen.width, screenPos.y * Screen.height), null, out canvasPos);
        if (isEnemyAtback)
        {
            if (screenPos.y < 0.5f)
            {
                canvasPos.y = CanvasPointer.GetComponent<RectTransform>().sizeDelta.y / 2;
            }
            else if (screenPos.y >= 0.5f)
            {
                canvasPos.y = -CanvasPointer.GetComponent<RectTransform>().sizeDelta.y / 2;
            }
            if (screenPos.x < 0.5f)
            {
                canvasPos.x = CanvasPointer.GetComponent<RectTransform>().sizeDelta.x / 2;
            }
            else if (screenPos.x >= 0.5f)
            {
                canvasPos.x = -CanvasPointer.GetComponent<RectTransform>().sizeDelta.x / 2;
            }
        }

        canvasPos.x = Mathf.Clamp(canvasPos.x, -CanvasPointer.GetComponent<RectTransform>().sizeDelta.x / 2 + margin, CanvasPointer.GetComponent<RectTransform>().sizeDelta.x / 2 - margin);
        canvasPos.y = Mathf.Clamp(canvasPos.y, -CanvasPointer.GetComponent<RectTransform>().sizeDelta.y / 2 + margin, CanvasPointer.GetComponent<RectTransform>().sizeDelta.y / 2 - margin);
        EnemyPointerOutside.anchoredPosition = canvasPos;
    }

    void UpdatePointerInsidePosition(Vector3 screenPos)
    {
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasPointer.GetComponent<RectTransform>(), new Vector2(screenPos.x * Screen.width, screenPos.y * Screen.height), null, out canvasPos);

        canvasPos.x = Mathf.Clamp(canvasPos.x, -CanvasPointer.GetComponent<RectTransform>().sizeDelta.x / 2 + margin, CanvasPointer.GetComponent<RectTransform>().sizeDelta.x / 2 - margin);
        canvasPos.y = Mathf.Clamp(canvasPos.y, -CanvasPointer.GetComponent<RectTransform>().sizeDelta.y / 2 + margin, CanvasPointer.GetComponent<RectTransform>().sizeDelta.y / 2 - margin);
        EnemyPointerInside.anchoredPosition = canvasPos;
    }

}

