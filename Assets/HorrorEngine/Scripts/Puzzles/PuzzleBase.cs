using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorEngine
{
    public class PuzzleBase : MonoBehaviour, ISavableObjectStateExtra
    {
        public UnityEvent OnSolved;
        public UnityEvent OnLoadedSolved;

        protected bool m_Solved;

        public bool IsSolved { 
            get { return m_Solved; }
            private set { m_Solved = value; }
        }

        public string GetSavableData()
        {
            return m_Solved.ToString();
        }

        public void SetFromSavedData(string savedData)
        {
            m_Solved = Convert.ToBoolean(savedData);
            if (m_Solved)
                OnLoadedSolved?.Invoke();
        }

        public void Solve()
        {
            m_Solved = true;
            OnSolved?.Invoke();
        }
    }

}