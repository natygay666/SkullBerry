using UnityEngine;
using UnityEditor;

namespace HorrorEngine
{
    public class DocumentationEditor : MonoBehaviour
    {
        [MenuItem("Horror Engine/Documentation")]
        static void Open()
        {
            string docsLinks = "https://www.notion.so/Retro-Horror-Template-6c9c55d8a82e4b858906eca17f3edaf9?pvs=4";
            Application.OpenURL(docsLinks);
        }
    }
}