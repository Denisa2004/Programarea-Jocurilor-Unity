using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace Art_Controller
{
    /*General Code Could Be Used In Your Projects*/

    //State of Character
    public enum State { Idle, Walk, Attack, Skill, SecondSkill, Die };
    //State of Character Skill
    public enum ActionType { Attack, Skill, SecondSkill}

    //Character Information
    [System.Serializable]
    public class CharacterInfo
    {
        //The Prefab of Character
        public GameObject prefab;
        //Certain Character will spawn prefab in action, here is the list of the skills (including prefab, detail info of each skill)
        public List<ActionInfo> actions;
        //The Position Where the action prefab will be spawn from the position of the character
        public Vector3 characterOffset;
        //The Sfx info of each action of character
        public List<ActionSfx> actionSfx;
    }
    [System.Serializable]

    //Action Information for Certain Character that spawn prefab in action
    public class ActionInfo
    {
        //The type of action
        public ActionType type;
        //The prefab to be spawned in the action
        public GameObject prefab;
        //Delay Time Before the prefab spawn
        public float delay;
        //The flying speed of prefab before Hit, set to zero(0) if the prefab have no flying time (Direct Hit)
        //Set to (-1) if the prefab spawn and Hit on character
        public float flyingSpeed;
        //If want the skill to rotate around target, fill in the rotate duration. 0 means not rotating.
        public float rotateDuration;
        //If skill spawn position depends on monster, set this
        public Vector3 monsterOffset;
    }

    [System.Serializable]
    public class ActionSfx
    {
        //The type of State
        public State state;
        //Times of Sfx played According to Length of List, Delay time is before each Sfx played
        public List<float> delay;
        //Sfx to replace the original
        public AudioClip replacementSFX;
    }
    /*End of General Code*/

    /*Demo Code*/
    public class PrefabSpawner : MonoBehaviour
    {

        public List<CharacterInfo> characterInfo;
        public float prefabScale;
        private GameObject currentCharacter = null;
        private int currentIndex = -1;
        public GameObject spawnPoint;
        public GameObject stopPoint;
        public State state = State.Idle;
        public List<Sprite> buttonSprites;
        public Image buttonImage;
        public Animator characterAnimator = null;
        public GameObject summonPrefab;
        public Transform monsterTransform;
        public Vector3 monsterOffset;

        //After Spawn Button is Clicked
        public void SpawnCharacterButtonClicked(int index)
        {
            // Destroy existing Characters in Scene
            if (currentCharacter != null) RemoveCharacter();
            currentIndex = index;
            // Spawn new prefab according to index
            StartCoroutine(SummonCircle());
        }

        public void SpawnPrefabCharacter()
        {
            currentCharacter = Instantiate(characterInfo[currentIndex].prefab, spawnPoint.transform.position, Quaternion.identity);
            characterAnimator = currentCharacter.GetComponent<Animator>();
            state = State.Idle;
            characterAnimator.Play("idle");
            StartCoroutine(Fade(true));
        }

        public void RemoveCharacter()
        {
            // Destroy existing Characters in Scene
            Destroy(currentCharacter);
            currentCharacter = null;
            characterAnimator = null;
            state = State.Idle;
        }

        private IEnumerator SpawnSkill(ActionInfo skillInfo)
        {
            yield return new WaitForSeconds(skillInfo.delay);
            Vector3 monsterPosition = monsterTransform.position + monsterOffset;
            Vector3 spawnPosition = skillInfo.monsterOffset == Vector3.zero ?
                currentCharacter.transform.position + characterInfo[currentIndex].characterOffset : monsterPosition + skillInfo.monsterOffset;
            GameObject skillPrefab = Instantiate(skillInfo.prefab, spawnPosition, Quaternion.identity);
            Animator skillAnimator = skillPrefab.GetComponent<Animator>();
            //Skill Flying
            if (skillInfo.flyingSpeed > 0)
            {
                if(skillAnimator!=null)
                    skillAnimator.Play("flying");
                while (Vector3.Distance(skillPrefab.transform.position, monsterTransform.position + monsterOffset) > Mathf.Epsilon)
                {
                    skillPrefab.transform.position = Vector3.MoveTowards(skillPrefab.transform.position, monsterPosition, Time.deltaTime * skillInfo.flyingSpeed);
                    yield return null;
                    
                }
            }
            //Skill Hit
            if(skillInfo.flyingSpeed == 0)
            {
                skillPrefab.transform.position = monsterPosition;
            }
            if (skillAnimator != null)
                skillAnimator.Play("hit");
            //Skill Hit SFX
            AudioSource skillSFX = skillPrefab.GetComponent<AudioSource>();
            if (skillSFX != null)
                skillSFX.PlayOneShot(skillSFX.clip);
            if(skillInfo.rotateDuration > 0)
            {
                //Rotate of skill
                Quaternion startRotation = skillPrefab.transform.rotation;
                Quaternion endRotation = Quaternion.Euler(0, 0, 180); 
                float elapsedTime = 0f;

                while (elapsedTime < skillInfo.rotateDuration)
                {
                    skillPrefab.transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / skillInfo.rotateDuration);
                    elapsedTime += Time.deltaTime*5;
                    yield return null;
                }
            }
            else if (skillAnimator != null)
                yield return new WaitUntil(() => skillAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
            //Fade
            var spriteRenderer = skillPrefab.GetComponent<SpriteRenderer>();
            float transparency = spriteRenderer.color.a;
            while (transparency > 0)
            {
                transparency -= 0.1f;
                spriteRenderer.color = new Color(1, 1, 1, transparency);
                yield return new WaitForSeconds(0.1f);
            }
            Destroy(skillPrefab);
        }

        //After Action Button is Clicked
        public void ActionChange()
        {
            if (currentCharacter == null) return;
            var animatorController = characterAnimator.runtimeAnimatorController as AnimatorController;
            var states = animatorController.layers.SelectMany(layer => layer.stateMachine.states);
            if (state == State.Idle)
            {
                characterAnimator.Play("walk");
                state = State.Walk;
            }
            else if (state == State.Walk && states.Any(x=>x.state.name == "attack"))
            {
                //Attack Animation
                characterAnimator.Play("attack");
                state = State.Attack;
                //Attack Sfx
                StartCoroutine(ActionSFX());
                //Attack Prefab
                if (characterInfo[currentIndex].actions.Count > 0 && characterInfo[currentIndex].actions.Any(x => x.type == ActionType.Attack))
                {
                    List<ActionInfo> actions = characterInfo[currentIndex].actions.Where(x => x.type == ActionType.Attack).ToList();
                    //Spawn All listed Prefab, remember for the delay for each spawned prefab
                    for (int i = 0; i < actions.Count; i++)
                    {
                        StartCoroutine(SpawnSkill(actions[i]));
                    }
                }   
            }
            else if (state == State.Attack && states.Any(x => x.state.name == "skill"))
            {
                //Skill Animation
                characterAnimator.Play("skill");
                state = State.Skill;
                //Skill Sfx
                StartCoroutine(ActionSFX());
                //Skill Prefab
                if (characterInfo[currentIndex].actions.Count > 0 && characterInfo[currentIndex].actions.Any(x => x.type == ActionType.Skill))
                {
                    List<ActionInfo> actions = characterInfo[currentIndex].actions.Where(x => x.type == ActionType.Skill).ToList();
                    //Spawn All listed Prefab, remember for the delay for each spawned prefab
                    for (int i = 0; i < actions.Count; i++)
                    {
                        StartCoroutine(SpawnSkill(actions[i]));
                    }
                }
            }
            else if (state == State.Skill && states.Any(x => x.state.name == "secondSkill"))
            {
                //SecondSkill Animation
                characterAnimator.Play("secondSkill");
                state = State.SecondSkill;
                //SecondSkill Sfx
                StartCoroutine(ActionSFX());
                //Second Skill Prefab
                if (characterInfo[currentIndex].actions.Count > 0 && characterInfo[currentIndex].actions.Any(x => x.type == ActionType.SecondSkill))
                {
                    List<ActionInfo> actions = characterInfo[currentIndex].actions.Where(x => x.type == ActionType.SecondSkill).ToList();
                    //Spawn All listed Prefab, remember for the delay for each spawned prefab
                    for (int i = 0; i < actions.Count; i++)
                    {
                        StartCoroutine(SpawnSkill(actions[i]));
                    }
                }
            }
            else 
            {
                characterAnimator.Play("die");
                state = State.Die;
                StartCoroutine(Fade(false));
            }
        }

        private void Update()
        {
            buttonImage.sprite = buttonSprites[(int)state];
            if (characterAnimator != null) 
            {
                //Set Front or Back Sprite
                characterAnimator.SetFloat("movementY", -1);
            }
            if (state == State.Walk)
            {
                currentCharacter.transform.position = Vector3.MoveTowards(currentCharacter.transform.position, stopPoint.transform.position, Time.deltaTime*2);
                if(currentCharacter.transform.position == stopPoint.transform.position) characterAnimator.Play("idle");
            }
        }


        //Demo Effects
        public IEnumerator SummonCircle()
        {
            Vector3 position = spawnPoint.transform.position;
            GameObject prefab = Instantiate(summonPrefab, new Vector3(position.x, position.y+0.3f, position.z), Quaternion.identity);
            SpriteRenderer spriteRenderer = prefab.GetComponent<SpriteRenderer>();
            float transparency = spriteRenderer.color.a;
            Animator animator = prefab.GetComponent<Animator>();
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5);
            prefab.GetComponent<AudioSource>().PlayOneShot(prefab.GetComponent<AudioSource>().clip) ;
            float duration = animator.GetCurrentAnimatorStateInfo(0).length;
            SpawnPrefabCharacter();
            while (transparency > 0)
            {
                transparency -= 0.1f;
                spriteRenderer.color = new Color(1, 1, 1, transparency);
                yield return new WaitForSeconds(duration * 0.1f);
            }
            Destroy(prefab);
        }
        //Demo Effects Fade
        public IEnumerator Fade(bool fadeIn)
        {
            SpriteRenderer spriteRenderer = currentCharacter.GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(1, 1, 1, fadeIn ? 0 : 1);
            float transparency = spriteRenderer.color.a;
            while (fadeIn ? transparency < 1 : transparency > 0)
            {
                transparency += fadeIn ? 0.1f : -0.1f;
                spriteRenderer.color = new Color(1, 1, 1, transparency);
                yield return new WaitForSeconds(0.1f);
            }
            if (!fadeIn) RemoveCharacter();
        }

        public IEnumerator ActionSFX()
        {
            ActionSfx currentActionSfx = characterInfo[currentIndex].actionSfx.FirstOrDefault(x=>x.state == state);
            if (currentActionSfx == null) yield break;
            AudioSource characterAudio = currentCharacter.GetComponent<AudioSource>();
            AudioClip clipToPlay = currentActionSfx.replacementSFX != null ? currentActionSfx.replacementSFX : characterAudio.clip;
            if (characterAudio == null) yield break;
            foreach (float second in currentActionSfx.delay)
            {
                yield return new WaitForSeconds(second);
                characterAudio.PlayOneShot(clipToPlay);
            }
        }
    }

}
