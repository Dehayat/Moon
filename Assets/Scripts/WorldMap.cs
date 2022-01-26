using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Edge
{
    public Node start;
    public Node end;
}


public struct Graph
{
    List<int>[] graph;
    bool[] visited;
    Node[] nodes;

    public Graph(Node[] nodes, Edge[] edges)
    {
        if (nodes == null || nodes.Length == 0)
        {
            Debug.LogError("Graph is Empty");
            graph = null;
        }
        this.nodes = nodes;
        graph = new List<int>[nodes.Length];
        visited = new bool[nodes.Length];
        ClearVisited();
        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i].id = i;
            graph[i] = new List<int>();
        }
        if (edges != null)
        {
            for (int i = 0; i < edges.Length; i++)
            {
                if (edges[i].start.id == edges[i].end.id)
                {
                    Debug.LogError("Edge " + i + " has the same start and end node " + edges[i].start.id);
                }
                else
                {
                    if (graph[edges[i].start.id].Contains(edges[i].end.id))
                    {
                        Debug.LogError("Edge " + i + " is a duplicate edge that connects nodes " + edges[i].start.id + " and " + edges[i].end.id);
                    }
                    else
                    {
                        graph[edges[i].start.id].Add(edges[i].end.id);
                        graph[edges[i].end.id].Add(edges[i].start.id);
                    }
                }
            }
        }
    }

    public List<Node> GetNodesInRange(Node startNode, int maxDistance)
    {
        ClearVisited();
        BFS(startNode.id, maxDistance);
        List<Node> nodesInRange = new List<Node>();
        for (int i = 0; i < visited.Length; i++)
        {
            if (i == startNode.id) continue;
            if (visited[i])
            {
                nodesInRange.Add(nodes[i]);
            }
        }
        return nodesInRange;
    }
    private struct queuedNode
    {
        public int nodeId;
        public int distance;
        public queuedNode(int id, int distance)
        {
            nodeId = id;
            this.distance = distance;
        }
    }
    private void BFS(int startNode, int maxDistance = 100000)
    {
        Queue<queuedNode> nextNodes = new Queue<queuedNode>();
        nextNodes.Enqueue(new queuedNode(startNode, 0));
        visited[startNode] = true;
        while (nextNodes.Count > 0)
        {
            queuedNode currentNode = nextNodes.Dequeue();
            int nextDistance = currentNode.distance + 1;
            for (int i = 0; i < graph[currentNode.nodeId].Count; i++)
            {
                int nextNodeId = graph[currentNode.nodeId][i];
                if (visited[nextNodeId]) continue;
                visited[nextNodeId] = true;
                if (nextDistance < maxDistance)
                {
                    nextNodes.Enqueue(new queuedNode(nextNodeId, nextDistance));
                }
            }
        }
    }

    private void ClearVisited()
    {
        for (int i = 0; i < visited.Length; i++)
        {
            visited[i] = false;
        }
    }

    public Character GetClosestHuman(int startNode, BloodType bloodType)
    {
        ClearVisited();
        return GetHumanBFS(startNode, bloodType);
    }

    private Character GetHumanBFS(int startNode, BloodType bloodType)
    {
        Queue<queuedNode> nextNodes = new Queue<queuedNode>();
        nextNodes.Enqueue(new queuedNode(startNode, 0));
        visited[startNode] = true;
        while (nextNodes.Count > 0)
        {
            queuedNode currentNode = nextNodes.Dequeue();
            for (int j = 0; j < nodes[currentNode.nodeId].charactersInNode.Count; j++)
            {
                var character = nodes[currentNode.nodeId].charactersInNode[j];
                if (character.isAlive && !character.isWolf && (bloodType == BloodType.IGNORE || (bloodType & character.bloodType.bloodType) != 0))
                {
                    return character;
                }
            }
            int nextDistance = currentNode.distance + 1;
            for (int i = 0; i < graph[currentNode.nodeId].Count; i++)
            {

                int nextNodeId = graph[currentNode.nodeId][i];
                if (visited[nextNodeId]) continue;
                visited[nextNodeId] = true;
                nextNodes.Enqueue(new queuedNode(nextNodeId, nextDistance));
            }
        }
        Debug.LogError("Cannot find humans with bloodtype preference " + bloodType);
        return null;
    }
}

public class WorldMap : MonoBehaviour
{
    public Node[] nodes;
    public Edge[] edges;

    public List<Node> GetNodesInRange(Node startNode, int maxDistance)
    {
        return graph.GetNodesInRange(startNode, maxDistance);
    }
    public List<Character> GetCharactersInRange(Node currentNode, int range)
    {
        List<Character> charactersInRange = new List<Character>();
        var nodesInRange = GetNodesInRange(currentNode, range);
        for (int i = 0; i < nodesInRange.Count; i++)
        {
            for (int j = 0; j < nodesInRange[i].charactersInNode.Count; j++)
            {
                if (nodesInRange[i].charactersInNode[j].isAlive)
                {
                    charactersInRange.Add(nodesInRange[i].charactersInNode[j]);
                }
            }
        }
        return charactersInRange;
    }

    private Graph graph;
    private void Awake()
    {
        graph = new Graph(nodes, edges);
    }


    private void OnDrawGizmos()
    {
        if (edges != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < edges.Length; i++)
            {
                if (edges[i].start != null && edges[i].end != null)
                {
                    Gizmos.DrawLine(edges[i].start.transform.position, edges[i].end.transform.position);
                }
            }
        }
    }

    public Character FindHumanToAttack(Node node, BloodType bloodTypePreference)
    {
        if (WorldData.instance.cycleManager.CanFindBloodType(bloodTypePreference))
        {
            return graph.GetClosestHuman(node.id, bloodTypePreference);
        }
        else
        {
            return graph.GetClosestHuman(node.id, BloodType.IGNORE);
        }
    }
}
