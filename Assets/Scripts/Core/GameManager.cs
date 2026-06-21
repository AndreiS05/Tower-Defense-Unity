using System;
using UnityEngine;

namespace TowerDefense
{
    /// <summary>
    /// Sistemul central de gestionare a jocului: ține evidența resurselor (bani),
    /// a vieților bazei și a stării partidei (în desfășurare / victorie / înfrângere).
    /// Este un singleton accesibil global prin <see cref="Instance"/>.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState { Playing, Victory, Defeat }

        [Header("Setări inițiale")]
        public int startingLives = 20;
        public int startingMoney = 200;

        public int Lives { get; private set; }
        public int Money { get; private set; }
        public GameState State { get; private set; }

        // Evenimente pentru ca interfața (UI) să se actualizeze automat.
        public event Action<int> OnMoneyChanged;
        public event Action<int> OnLivesChanged;
        public event Action<GameState> OnStateChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Lives = startingLives;
            Money = startingMoney;
            State = GameState.Playing;
        }

        void Start()
        {
            // Notifică UI-ul cu valorile inițiale.
            OnMoneyChanged?.Invoke(Money);
            OnLivesChanged?.Invoke(Lives);
            OnStateChanged?.Invoke(State);
        }

        /// <summary>Adaugă bani (recompensă pentru distrugerea unui inamic).</summary>
        public void AddMoney(int amount)
        {
            if (State != GameState.Playing) return;
            Money += amount;
            OnMoneyChanged?.Invoke(Money);
        }

        /// <summary>Încearcă să cheltuie o sumă; întoarce false dacă fondurile sunt insuficiente.</summary>
        public bool TrySpend(int amount)
        {
            if (State != GameState.Playing || Money < amount) return false;
            Money -= amount;
            OnMoneyChanged?.Invoke(Money);
            return true;
        }

        public bool CanAfford(int amount) => Money >= amount;

        /// <summary>Bazele pierd vieți când un inamic ajunge la punctul vital.</summary>
        public void LoseLives(int amount)
        {
            if (State != GameState.Playing) return;
            Lives = Mathf.Max(0, Lives - amount);
            OnLivesChanged?.Invoke(Lives);
            if (Lives <= 0) SetState(GameState.Defeat);
        }

        /// <summary>Apelat de WaveSpawner când toate valurile au fost eliminate.</summary>
        public void WinGame()
        {
            if (State == GameState.Playing) SetState(GameState.Victory);
        }

        void SetState(GameState newState)
        {
            State = newState;
            OnStateChanged?.Invoke(State);
        }
    }
}
