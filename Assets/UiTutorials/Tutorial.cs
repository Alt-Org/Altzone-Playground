using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;

public class Tutorial : MonoBehaviour
{
    [SerializeField] internal GameObject TutorialParent;
    [SerializeField] internal string TutorialText;
    [SerializeField] internal float CharacterInterval;
    [SerializeField] internal TextMeshProUGUI TutorialTextDisplay;
    internal int TextStep;
    internal AudioSource Speaker;
    internal bool TypingTutorial;
    [SerializeField] internal GameObject SkipButton;
    [SerializeField] internal GameObject CloseButton;

    void Awake()
    {
        Speaker = GetComponent<AudioSource>();

        

        PlayTutorial();

    }
    private void Update()
    {
        
    }
    public void SkipTutorial ()
    {
        StopAllCoroutines();
        Speaker.Stop();
        CloseButton.SetActive(true);
        SkipButton.SetActive(false);
        TypingTutorial = false;
        TutorialTextDisplay.text = TutorialText;
    }
    internal void LevelLoaded (bool LoadedForTheFirstTime)
    {
        if (LoadedForTheFirstTime)
        {
            PlayTutorial();
        }
        else
        {

        }
    }
    internal void PlayTutorial ()
    {
        TutorialParent.SetActive(true);
        CloseButton.SetActive(false);
        TypingTutorial = true;
        TutorialTextDisplay.text = "";
        Speaker.Play();
        StartCoroutine("PrintOutTutorial");
    }
    public void EndTutorial ()
    {
        TutorialParent.SetActive(false);
    }
    internal IEnumerator PrintOutTutorial ()
    {
        TextStep = 0;
        while (true)
        {
            float WaitTime = CharacterInterval;

            if (!Speaker.isPlaying)
            {
                Speaker.Play();
            }
            if (TextStep >= TutorialText.Length)
            {
                Speaker.Stop();
                TypingTutorial = false;
                CloseButton.SetActive(true);
                SkipButton.SetActive(false);
                StopAllCoroutines();
                break;
            }
            string MenuText = TutorialTextDisplay.text;
            MenuText += TutorialText[TextStep];
            TextStep++;
            TutorialTextDisplay.text = MenuText;
            if (TextStep < TutorialText.Length)
            {
                char NextLetter = TutorialText[TextStep];
                if (!char.IsLetterOrDigit(NextLetter))
                {
                    WaitTime = 0;
                }
            }

            else
            {
                Speaker.Pause();
            }
            yield return new WaitForSeconds(WaitTime);
        }
    }
    
}
