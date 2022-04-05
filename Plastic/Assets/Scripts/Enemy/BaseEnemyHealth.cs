using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseEnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyStats enemyStats;

    [SerializeField] private Slider healthbarSlider;
    [SerializeField] private Image healthbarFillImage;

    [SerializeField] private Color maxHealthColor;
    [SerializeField] private Color zeroHealthColor;

    private int currentHealth;


    private void Start()
    {
        currentHealth = enemyStats.enemyMaxHealth;
        SetHealthbarUI();
    }

    public void DealDamage(int damage)
    {
        currentHealth -= damage;
        checkIfDead();
        SetHealthbarUI();
    }

    private void checkIfDead()
    {
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Destroy(gameObject);
        }
    }

    private void SetHealthbarUI()
    {
        float healthPercentage = calculateHealthPercentage();
        healthbarSlider.value = healthPercentage;
        healthbarFillImage.color = Color.Lerp(zeroHealthColor, maxHealthColor, healthPercentage / 100);
    }

    private float calculateHealthPercentage()
    {
        return ((float)currentHealth / (float)enemyStats.enemyMaxHealth) * 100;
    }

}
