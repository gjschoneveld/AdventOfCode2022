﻿Dictionary<char, Hand> handMap = new()
{
    ['A'] = Hand.Rock,
    ['B'] = Hand.Paper,
    ['C'] = Hand.Scissors,
    ['X'] = Hand.Rock,
    ['Y'] = Hand.Paper,
    ['Z'] = Hand.Scissors,
};

Dictionary<char, Outcome> outcomeMap = new()
{
    ['X'] = Outcome.Lost,
    ['Y'] = Outcome.Draw,
    ['Z'] = Outcome.Won,
};

Dictionary<(Hand opponent, Hand self), Outcome> outcomes = new()
{
    [(Hand.Rock, Hand.Rock)] = Outcome.Draw,
    [(Hand.Rock, Hand.Paper)] = Outcome.Won,
    [(Hand.Rock, Hand.Scissors)] = Outcome.Lost,
    [(Hand.Paper, Hand.Rock)] = Outcome.Lost,
    [(Hand.Paper, Hand.Paper)] = Outcome.Draw,
    [(Hand.Paper, Hand.Scissors)] = Outcome.Won,
    [(Hand.Scissors, Hand.Rock)] = Outcome.Won,
    [(Hand.Scissors, Hand.Paper)] = Outcome.Lost,
    [(Hand.Scissors, Hand.Scissors)] = Outcome.Draw
};

int Score((Hand opponent, Hand self) round)
{
    return (int)outcomes[round] + (int)round.self;
}

Hand Needed(Hand opponent, Outcome outcome)
{
    return outcomes.First(kv => kv.Key.opponent == opponent && kv.Value == outcome).Key.self;
}

var input = File.ReadAllLines("input.txt");
var rounds = input.Select(line => (opponent: line[0], self: line[^1])).ToList();

var answer1 = rounds.Select(r => (opponent: handMap[r.opponent], self: handMap[r.self])).Sum(Score);
Console.WriteLine($"Answer 1: {answer1}");

var answer2 = rounds.Select(r => (opponent: handMap[r.opponent], self: Needed(handMap[r.opponent], outcomeMap[r.self]))).Sum(Score);
Console.WriteLine($"Answer 2: {answer2}");

enum Hand
{
    Rock = 1,
    Paper = 2,
    Scissors = 3
}

enum Outcome
{
    Lost = 0,
    Draw = 3,
    Won = 6
}