using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class EnemyHealthBar : MonoBehaviour
{
    private Vector2 positionCorrection = new Vector2(0, 100);

    [SerializeField]
    private RectTransform targetCanvas;
    [SerializeField]
    private RectTransform healthBar;
    [SerializeField]
    private Enemy enemy;

    public void SetHealthBarData(Enemy target, RectTransform healthBarPanel)
    {
        this.targetCanvas = healthBarPanel;
        healthBar = GetComponent<RectTransform>();
        enemy = target;
        RepositionHealthBar();
        CalculateFill();
        healthBar.gameObject.SetActive(true);
    }

    private void OnHealthChanged(float healthFill)
    {
        healthBar.GetComponent<Image>().fillAmount = healthFill;
    }

    public void CalculateFill()
    {
        float currHealth = enemy.GetCurrentHealth(),
            maxHealth = enemy.GetTotalHealth();
        float fill = (float)System.Math.Round(
            value: currHealth / maxHealth,
            digits: 1);
        if (fill != healthBar.GetComponent<Image>().fillAmount)
            OnHealthChanged(fill);
    }

    private void RepositionHealthBar()
    {
        float headPosition = enemy.transform.TransformPoint(enemy.GetComponents<PolygonCollider2D>()
            .FirstOrDefault(_ => _.enabled).points
            .OrderByDescending(x => x.y)
            .First()).y;
        Vector2 targetPosition = new Vector2(
            x: enemy.transform.position.x,
            y: headPosition + 0.2f);
        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(targetPosition);

        Vector2 WorldObject_ScreenPosition = new Vector2(
        ((ViewportPosition.x * targetCanvas.sizeDelta.x) - (targetCanvas.sizeDelta.x * 0.5f)),
        ((ViewportPosition.y * targetCanvas.sizeDelta.y) - (targetCanvas.sizeDelta.y * 0.5f)));

        //now you can set the position of the ui element
        healthBar.anchoredPosition = WorldObject_ScreenPosition;
    }

    // Update is called once per frame
    void Update()
    {
        RepositionHealthBar();
        CalculateFill();
    }
}
