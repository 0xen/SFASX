using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalEntity : Entity
{
    // Actions the animal can do
    public enum AnimalActions
    {
        Walk,
        Breed
    }

    // A list of actions the current animal can do
    [SerializeField] private AnimalActions[] actions = null;

    // How many tiles the animal can walk each update
    [SerializeField] private int walkRange = 0;
    
    // How long before the animal could breed
    [SerializeField] private float breedingTimer = 0.0f;

    // How far will the animal look to breed
    [SerializeField] private int breedingRange = 0;

    // When the animal is spawned, whats the minimum value the breeding time starts at
    [SerializeField] private float minBreedTimerStart = 0.0f;

    // When the animal is spawned, whats the max value the breeding time starts at
    [SerializeField] private float maxBreedTimerStart = 0.0f;

    // Instance of the animals breeding heart effect
    public ParticleSystem breedHeartEffect = null;

    // Minimum time before a action can be taken
    [SerializeField] private float minTimeBeforeAction = 0.0f;

    // Max time before a action can be taken
    [SerializeField] private float maxTimeBeforeAction = 0.0f;

    // How long until the next action can be taken
    private float actionDelta;

    // How long until the animal could breed
    private float m_breedingTimerDelta;

    // Is the animal currently preforming a action
    private bool mPreformingAction;


    public AnimalEntity() : base(1)
    {
        mPreformingAction = false;
    } 

    public void Start()
    {
        actionDelta = Random.Range(minTimeBeforeAction, maxTimeBeforeAction);
        m_breedingTimerDelta = Random.Range(minBreedTimerStart, maxBreedTimerStart);
        breedHeartEffect.Stop();
    }

    
    public void ResetBreedingTimer()
    {
        m_breedingTimerDelta = 0;
    }

    private void Update()
    {
        m_breedingTimerDelta += Time.deltaTime;
        // If the timer for actions has elapsed
        if (CanPreformAction())
        {
            // Make sure the animal has a action
            if (actions.Length > 0)
            {
                // Chose a action randomly
                switch (actions[Random.Range(0, actions.Length)])
                {
                    // Make the action walk x distance
                    case AnimalActions.Walk:
                        Walk(walkRange);
                        break;
                    // Attempt to breed with another animal
                    case AnimalActions.Breed:
                        // If the breeding timer has elapsed, breed
                        if (m_breedingTimerDelta > breedingTimer) 
                        {
                            Breed(breedingRange);
                        }
                        else
                        {
                            mPreformingAction = false;
                        }
                        break;

                }
            }
        }
    }

    // Get the item in the players hand
    public override Item GetHandItem()
    {
        return inventory[0];
    }

    // Called on a inventory change, count represents the amount of items inserted or removed
    public override void InventoryChange(Item item, uint count, InventoryChangeEvent eve)
    {

    }

    // Change the entities current animation
    public override void ChangeAnimation(AnimationStates state)
    {

    }

    // Try and find a another animal of the same breed in x range and make ....love.... i guess xD
    public void Breed(int range)
    {
        Vector2Int mapSize = Environment.instance.mMapGenerationPayload.size;
        // Calculate the mins and maxes for the possible touching tile coordinates
        int xMin = CurrentPosition.PositionTile.x - range < 0 ? 0 : CurrentPosition.PositionTile.x - range;
        int yMin = CurrentPosition.PositionTile.y - range < 0 ? 0 : CurrentPosition.PositionTile.y - range;
        int xMax = CurrentPosition.PositionTile.x + range < mapSize.x ? CurrentPosition.PositionTile.x + range : mapSize.x - 1;
        int yMax = CurrentPosition.PositionTile.y + range < mapSize.y ? CurrentPosition.PositionTile.y + range : mapSize.y - 1;

        List<AnimalEntity> animalsInRange = new List<AnimalEntity>();

        foreach(AnimalEntity entity in Environment.instance.GetEntities())
        {
            EnvironmentTile entityTile = entity.CurrentPosition;
            if(entity != this && entity.entityName == this.entityName && 
                entityTile.PositionTile.x >= xMin && entityTile.PositionTile.x <= xMax &&
                entityTile.PositionTile.y >= yMin && entityTile.PositionTile.y <= yMax)
            {
                animalsInRange.Add(entity);
            }
        }

        if(animalsInRange.Count>0)
        {
            AnimalEntity entity = animalsInRange[Random.Range(0, animalsInRange.Count)];
            List<EnvironmentTile> route = Environment.instance.SolveNeighbour(CurrentPosition, entity.CurrentPosition);

            // If the animal is already there, break
            if (route == null)
            {
                mPreformingAction = false;
                return;
            }
            if (route.Count > 0)
            {
                StopAllCoroutines();
                StartCoroutine(DoWalkAndBreed(route, entity.CurrentPosition, entity));
            }
            else
            {
                mPreformingAction = false;
                return;
            }


            m_breedingTimerDelta = 0.0f;
        }
        mPreformingAction = false;
    }

    // Walk x range from its current location to a random free tile if available
    public void Walk(int range)
    {
        Vector2Int mapSize = Environment.instance.mMapGenerationPayload.size;
        // Calculate the mins and maxes for the possible touching tile coordinates
        int xMin = CurrentPosition.PositionTile.x - range < 0 ? 0 : CurrentPosition.PositionTile.x - range;
        int yMin = CurrentPosition.PositionTile.y - range < 0 ? 0 : CurrentPosition.PositionTile.y - range;
        int xMax = CurrentPosition.PositionTile.x + range < mapSize.x ? CurrentPosition.PositionTile.x + range : mapSize.x - 1;
        int yMax = CurrentPosition.PositionTile.y + range < mapSize.y ? CurrentPosition.PositionTile.y + range : mapSize.y - 1;


        int randomX = Random.Range(xMin, xMax);
        int randomY = Random.Range(yMin, yMax);

        EnvironmentTile destination = Environment.instance.GetTile(randomX, randomY);

        List<EnvironmentTile> route = Environment.instance.Solve(CurrentPosition, destination);

        // If the animal is already there, break
        if (route == null)
        {
            mPreformingAction = false;
            return;
        }
        if (route.Count > 0)
        {
            StopAllCoroutines();
            StartCoroutine(DoWalk(route, destination));
        }
        else
        {
            mPreformingAction = false;
            return;
        }

    }

    // Preform the walk action
    private IEnumerator DoWalk(List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(this, GetMovmentSpeed(), route);
        mPreformingAction = false;
    }

    // Walk to the partners tile and breform the breed action
    private IEnumerator DoWalkAndBreed(List<EnvironmentTile> route, EnvironmentTile tile, AnimalEntity partner)
    {
        yield return TileActionWalk.DoGoTo(this, GetMovmentSpeed(), route);

        partner.ResetBreedingTimer();
        partner.breedHeartEffect.Play();
        breedHeartEffect.Play();

        // Create new animal
        {
            AnimalEntity ent = GameObject.Instantiate(this, Environment.instance.transform);

            Environment.instance.RegisterEntity(ent);

            ent.ResetBreedingTimer();
            ent.transform.position = tile.Position;
            ent.transform.rotation = Quaternion.identity;
            ent.CurrentPosition = tile;
        }


        mPreformingAction = false;
    }

    // Check to see if we can preform a action
    public bool CanPreformAction()
    {
        if (mPreformingAction) return false;
        actionDelta -= Time.deltaTime;
        if (actionDelta<0)
        {
            actionDelta = Random.Range(minTimeBeforeAction, maxTimeBeforeAction);
            mPreformingAction = true;
            return true;
        }
        return false;
    }

}
