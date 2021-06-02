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
        Reposition();
        Fill();
        healthBar.gameObject.SetActive(true);
    }

    //private void OnHealthChanged(float healthFill)
    //{
    //    healthBar.GetComponent<Image>().fillAmount = healthFill;
    //}

    public void Fill()
    {
        float currHealth = enemy.GetCurrentHealth(),
            maxHealth = enemy.GetTotalHealth();
        float fill = (float)System.Math.Round(
            value: currHealth / maxHealth,
            digits: 1);
        if (fill != healthBar.GetComponent<Image>().fillAmount)
            //OnHealthChanged(fill);
            healthBar.GetComponent<Image>().fillAmount = fill;

        Debug.Log(
            message: enemy.name + " current health: \'" + currHealth / maxHealth + "%\' health bar current fill: \'" + fill + "%\'");
    }

    public void Reposition()
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
        Debug.Log(
            message: enemy.name + " health bar position changed to: " + WorldObject_ScreenPosition.ToString());
    }

    // Update is called once per frame
    //void Update()
    //{
    //    Reposition();
    //    Fill();
    //}
    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}
