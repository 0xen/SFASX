using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalEntity : Entity
{
    
    public enum AnimalActions
    {
        Walk,
        Breed
    }


    [SerializeField] private AnimalActions[] actions = null;

    [SerializeField] private int walkRange = 0;
    
    [SerializeField] private float breedingTimer = 0.0f;

    [SerializeField] private int breedingRange = 0;

    [SerializeField] private float minBreedTimerStart = 0.0f;

    [SerializeField] private float maxBreedTimerStart = 0.0f;

    public ParticleSystem breedHeartEffect = null;

    [SerializeField] private float minTimeBeforeAction = 0.0f;

    [SerializeField] private float maxTimeBeforeAction = 0.0f;

    private float actionDelta;

    private float m_breedingTimerDelta;

    private bool mPreformingAction;

    const int AnimalInventorySize = 1;


    public AnimalEntity() : base(AnimalInventorySize)
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
        if (CanPreformAction())
        {
            if (actions.Length > 0)
            {
                switch (actions[Random.Range(0, actions.Length)])
                {
                    case AnimalActions.Walk:
                        Walk(walkRange);
                        break;
                    case AnimalActions.Breed:
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
    
    public override Item GetHandItem()
    {
        return inventory[0];
    }

    public override void InventoryChange(Item item, uint count, InventoryChangeEvent eve)
    {

    }
    public override void ChangeAnimation(AnimationStates state)
    {

    }

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

    private IEnumerator DoWalk(List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(this, GetMovmentSpeed(), route);
        mPreformingAction = false;
    }

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
