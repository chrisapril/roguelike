﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphMap;
using System.Collections.Generic;
using QuickGraph;
using System.Linq;

namespace TestGraphMap
{
    [TestClass]
    public class MapModelTest
    {
        [TestMethod]
        public void MapCycleReducerRemovesOneCycleInInputMap()
        {
            //Build a graph with one nested cycle

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(4, 5);
            newMap.AddRoomConnection(3, 5);
            
            newMap.AddRoomConnection(5, 6);

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            var roomMapping = cycleReducer.roomMappingToNoCycles;

            //Confirm that all the cycle nodes are mapped to the first node
            Assert.AreEqual(roomMapping[3], 3);
            Assert.AreEqual(roomMapping[4], 3);
            Assert.AreEqual(roomMapping[5], 3);
        }

        [TestMethod]
        public void AddingSameEdgeTwiceDoesntMakeTwoEdges()
        {
            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(1, 2);

            Assert.AreEqual(1, newMap.RoomConnectionGraph.EdgeCount);
        }

        [TestMethod]
        public void RetainedEdgeIsInMapAfterCycleReduction()
        {
            //Build a graph with one nested cycle

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(4, 5);
            newMap.AddRoomConnection(3, 5);
            
            newMap.AddRoomConnection(5, 6);

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            Assert.IsTrue(cycleReducer.IsEdgeInRoomsNoCycles(1, 2));
        }

        [TestMethod]
        public void EdgeTraversingCycleIsInMapAfterCycleReduction()
        {
            //Build a graph with one nested cycle

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(4, 5);
            newMap.AddRoomConnection(3, 5);

            newMap.AddRoomConnection(5, 6);

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            Assert.IsTrue(cycleReducer.IsEdgeInRoomsNoCycles(3, 6));
        }

        [TestMethod]
        public void SquashedEdgeIsNotInMapAfterCycleReduction()
        {
            //Build a graph with one nested cycle

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(4, 5);
            newMap.AddRoomConnection(3, 5);

            newMap.AddRoomConnection(5, 6);

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            Assert.IsFalse(cycleReducer.IsEdgeInRoomsNoCycles(3, 4));
        }
        

        [ExpectedException(typeof(ApplicationException))]
        [TestMethod]
        public void RemovedEdgeNotFoundInMapCycleReducer()
        {
            //Build a graph with one nested cycle

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(4, 5);
            newMap.AddRoomConnection(3, 5);

            newMap.AddRoomConnection(5, 6);

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            cycleReducer.GetEdgeBetweenRoomsNoCycles(4, 5);
        }

        [TestMethod]
        public void MapCycleReducerRemovesMultipleCyclesInInputMap()
        {
            //Build a graph with one two nested cycles

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(4, 5);
            newMap.AddRoomConnection(3, 5);

            newMap.AddRoomConnection(5, 6);

            //Cycle

            newMap.AddRoomConnection(6, 7);
            newMap.AddRoomConnection(7, 8);
            newMap.AddRoomConnection(8, 9);
            newMap.AddRoomConnection(9, 10);
            newMap.AddRoomConnection(10, 11);
            newMap.AddRoomConnection(11, 6);
            newMap.AddRoomConnection(9, 6);

            newMap.AddRoomConnection(11, 12);

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            var roomMapping = cycleReducer.roomMappingToNoCycles;

            //Confirm that all the first cycle nodes are mapped to the first node in the cycle
            Assert.AreEqual(3, roomMapping[3]);
            Assert.AreEqual(3, roomMapping[4]);
            Assert.AreEqual(3, roomMapping[5]);

            //Confirm that all the second cycle nodes are mapped to the first node in the second cycle
            Assert.AreEqual(6, roomMapping[6]);
            Assert.AreEqual(6, roomMapping[7]);
            Assert.AreEqual(6, roomMapping[8]);
            Assert.AreEqual(6, roomMapping[9]);
            Assert.AreEqual(6, roomMapping[10]);
            Assert.AreEqual(6, roomMapping[11]);
        }

        [TestMethod]
        public void RoomsOnSideOfOriginInSplitMapAreInSameComponentAndViceVersa() {

            ConnectivityMap standardMap = BuildStandardTestMap();
            MapSplitter splitter = new MapSplitter(standardMap.RoomConnectionGraph.Edges, standardMap.GetEdgeBetweenRooms(9, 10), 1);

            Assert.AreEqual(splitter.RoomComponentIndex(9), splitter.OriginComponentIndex);
            Assert.AreEqual(splitter.RoomComponentIndex(10), splitter.NonOriginComponentIndex);

        }

        [TestMethod]
        public void RoomsOnSideOfOriginInMultiplySplitMapAreInOneComponent()
        {

            ConnectivityMap standardMap = BuildStandardTestMap();
            var edgesToSplit = new List<TaggedEdge<int, string>>();
            edgesToSplit.Add(standardMap.GetEdgeBetweenRooms(10, 11));
            edgesToSplit.Add(standardMap.GetEdgeBetweenRooms(3, 5));

            MapSplitter splitter = new MapSplitter(standardMap.RoomConnectionGraph.Edges, edgesToSplit, 1);

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 7, 8, 9, 10 }), splitter.MapComponent(splitter.OriginComponentIndex).ToList());
        }

        [TestMethod]
        public void IntegrationTestMapModelCanBeBuiltWithAStartVertexInACycle()
        {
            var connectivityMap = BuildStandardTestMap();
            var mapModel = new MapModel(connectivityMap, 7);

            Assert.AreEqual(7, mapModel.StartVertex);
            Assert.AreEqual(2, mapModel.StartVertexNoCycleMap);
        }

        
        [TestMethod]
        public void ConnectionObjectsWithSameValueAreEqual()
        {
            var connection1 = new Connection(1, 2);
            var connection2 = new Connection(1, 2);

            Assert.AreEqual(connection1, connection2);
        
        }

        [TestMethod]
        public void NewConnectionObjectsCanBeUsedAsKeysInDictionaries()
        {
            var dict = new Dictionary<Connection, int>();

            dict.Add(new Connection(1, 2), 1);
            Assert.AreEqual(1, dict[new Connection(1, 2)]);
        }

        private ConnectivityMap BuildStandardTestMap() {

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);
            
            //Branch

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(3, 5);
            newMap.AddRoomConnection(5, 6);

            //Cycle

            newMap.AddRoomConnection(2, 8);
            newMap.AddRoomConnection(2, 7);
            newMap.AddRoomConnection(8, 9);
            newMap.AddRoomConnection(7, 9);

            //Post-cycle

            newMap.AddRoomConnection(9, 10);
            newMap.AddRoomConnection(10, 11);
            newMap.AddRoomConnection(11, 12);
            newMap.AddRoomConnection(11, 13);

            //2-way branches
            newMap.AddRoomConnection(13, 14);
            newMap.AddRoomConnection(13, 15);

            //Save to disk
            GraphvizExport.OutputUndirectedGraph(newMap.RoomConnectionGraph, "standard-test-map");

            return newMap;
        }

    }
}