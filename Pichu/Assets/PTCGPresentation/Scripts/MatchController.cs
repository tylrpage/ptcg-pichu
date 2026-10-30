using System;
using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core;
using tylrpage.Pichu.PTCGEngine.Core.Resolution;
using tylrpage.Pichu.PTCGEngine.Core.Rules;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    [SerializeField] private ulong _seed;
    [SerializeField] private int _pichus;
    [SerializeField] private int _rayquazas;
    [SerializeField] private int _fire;
    [SerializeField] private int _lightning;

    public event Action StateChanged;
    
    public bool IsSetup { get; private set; } = false;
    public List<GameEvent> Log { get; private set; } = new();
    public ICardDatabase Db { get; private set; }
    public GameState GameState { get; private set; }
    public ulong Seed => _seed;
    
    private List<PlayerCommand> _p1LegalCommandCache;
    private List<PlayerCommand> _p2LegalCommandCache;

    private void Awake()
    {
        Db = StarterCardSet.CreateDatabase();
        List<string> deckList = StarterCardSet.Deck(_pichus, _rayquazas, _lightning, _fire);
        GameState state = GameStateFactory.Create(Db, deckList, deckList);
        CommandResult result = Resolver.StartGame(state, _seed, Db);

        if (result.Status == CommandResult.CommandStatus.Rejected)
        {
            Debug.LogError($"Could not start game, status: {result.Status}, reason: {result.RejectReason}");
            return;
        }
        
        GameState = result.State;
        Log.AddRange(result.Events);
        IsSetup = true;

        UpdateLegalCommands();
    }

    public void Apply(PlayerCommand command)
    {
        if (!IsSetup)
        {
            Debug.LogError($"Ignoring command because game is not set up, command: {command.Type}");
            return;
        }

        CommandResult result = Resolver.Submit(GameState, command, Db);
        if (result.Status == CommandResult.CommandStatus.Rejected)
        {
            Debug.LogError($"Command was rejected, command: {command.Type}, reason: {result.RejectReason}");
            return;
        }

        GameState = result.State;
        Log.AddRange(result.Events);
        UpdateLegalCommands();
    }
    
    public List<PlayerCommand> GetLegalCommands(bool isPlayer1)
    {
        return isPlayer1 ? _p1LegalCommandCache : _p2LegalCommandCache;
    }

    private void UpdateLegalCommands()
    {
        _p1LegalCommandCache = RuleChecker.GetLegalCommands(GameState, true, Db);
        _p2LegalCommandCache = RuleChecker.GetLegalCommands(GameState, false, Db);
        StateChanged?.Invoke();
    }
}
