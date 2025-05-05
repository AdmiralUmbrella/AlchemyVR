using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthDisplayImageAuto : MonoBehaviour
{
    [Header("Referencias")]
    public MonoBehaviour enemySource;          // AI (p. ej. MeleeEnemyAI) o Data
    public Image        healthFillImage;       // Image → Type = Filled

    // Cachés para que la reflexión solo se haga una vez
    private object     healthHolder;           // Objeto donde realmente están los campos
    private FieldInfo  maxField;
    private FieldInfo  currField;

    void Awake()  => CacheHealthFields();

    void Start()  => UpdateUI();   // Una pasada inicial

    void Update()
    {
        UpdateUI();
        BillboardToCamera();
    }

    /* --------------------------------------------------------------------- */
    /* --------------------  Descubrimiento automático  -------------------- */
    /* --------------------------------------------------------------------- */
    private void CacheHealthFields()
    {
        if (enemySource == null) { Debug.LogError("EnemySource no asignado."); return; }

        Type sourceType = enemySource.GetType();

        // 1) ¿El propio componente tiene maxHealth / currentHealth?
        maxField  = sourceType.GetField("maxHealth");
        currField = sourceType.GetField("currentHealth");
        healthHolder = enemySource;

        // 2) Si no, busca un campo que termine en "Data" y contenga esos valores
        if (maxField == null || currField == null)
        {
            foreach (FieldInfo fi in sourceType.GetFields())
            {
                if (!fi.FieldType.Name.EndsWith("Data")) continue;

                object dataObj = fi.GetValue(enemySource);
                if (dataObj == null) continue;

                FieldInfo mf = fi.FieldType.GetField("maxHealth");
                FieldInfo cf = fi.FieldType.GetField("currentHealth");
                if (mf != null && cf != null)
                {
                    maxField     = mf;
                    currField    = cf;
                    healthHolder = dataObj;
                    break;
                }
            }
        }

        if (maxField == null || currField == null)
            Debug.LogError($"No pude encontrar maxHealth / currentHealth en {enemySource.name}. Revísalo.");
    }

    /* --------------------------------------------------------------------- */
    /* ---------------------------  Actualización  ------------------------- */
    /* --------------------------------------------------------------------- */
    private void UpdateUI()
    {
        if (maxField == null || currField == null) return;

        int max   = (int)maxField.GetValue(healthHolder);
        int curr  = (int)currField.GetValue(healthHolder);

        if (healthFillImage != null && max > 0)
            healthFillImage.fillAmount = (float)curr / max;
    }

    private void BillboardToCamera()
    {
        if (Camera.main != null)
            transform.rotation =
                Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }
}
