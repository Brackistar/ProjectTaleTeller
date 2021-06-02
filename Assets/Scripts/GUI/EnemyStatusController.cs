using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(AudioSource))]
public class EnemyStatusController : MonoBehaviour
{
    [SerializeField]
    [Range(0.1f, 1)]
    private float Time = 0.5f;
    private RectTransform Canvas;
    private Enemy Target;

    public void SetInitialData(Enemy target, RectTransform canvas)
    {
        Canvas = canvas;
        Target = target;

        //Target.OnDeath += OnEnemyDeath;

        Reposition();

        gameObject.SetActive(false);
    }

    public void SetInitialData(Enemy target, RectTransform canvas, float time)
    {
        Time = time;

        SetInitialData(target, canvas);
    }

    //private void Update()
    //{
    //    //if (!Target)
    //    //    OnEnemyDeath(Target);
    //    Reposition();
    //}
    public void Reposition()
    {
        float HeadPosition = Target.transform.TransformPoint(
            position: Target.gameObject.GetComponent<PolygonCollider2D>()
                .points
                .OrderByDescending(_ => _.y)
                .FirstOrDefault()).y;
        Vector2 FinalPosition = new Vector2(
            x: Target.transform.position.x,
            y: HeadPosition + 0.3f);
        Vector2 ViewPortPosition = Camera.main.WorldToViewportPoint(FinalPosition);

        Vector2 WorldObject_ScreenPosition = new Vector2(
            x: (ViewPortPosition.x * Canvas.sizeDelta.x) - (Canvas.sizeDelta.x * 0.5f),
            y: (ViewPortPosition.y * Canvas.sizeDelta.y) - (Canvas.sizeDelta.y * 0.5f));

        GetComponent<RectTransform>().anchoredPosition = WorldObject_ScreenPosition;

        Debug.Log(
            message: Target.name + " status indicator new position: " + WorldObject_ScreenPosition.ToString());
    }

    public void EnableAlert()
    {
        if (gameObject.activeSelf)
            return;

        gameObject.SetActive(true);
        GetComponent<AudioSource>().Play();

        StartCoroutine(StartAlert());
    }

    private IEnumerator StartAlert()
    {
        yield return new WaitForSeconds(Time);
        gameObject.SetActive(false);
    }

    //private void OnEnemyDeath(Enemy enemy)
    //{
    //    Destroy(this);
    //}

    private void OnDestroy()
    {
        Destroy(gameObject); ;
    }
}