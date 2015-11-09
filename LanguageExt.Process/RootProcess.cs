﻿using LanguageExt.UnitsOfMeasure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static LanguageExt.Prelude;
using static LanguageExt.Process;

namespace LanguageExt
{
    /// <summary>
    /// NOT IN USE
    /// This is a speculative idea for the root process to create a DNS like
    /// system for finding other nodes in the cluster.
    /// </summary>
    class RootProcess
    {
        const string MembersKey = "sys-cluster-members";
        const string RegisteredKey = "sys-dns";
        static readonly Time HeartbeatFreq = 5*seconds;

        public enum MsgTag
        {
            Heartbeat,
            ClusterMembersUpdated
        }

        public class Msg
        {
            public readonly MsgTag Tag;

            public Msg(MsgTag tag)
            {
                Tag = tag;
            }
        }

        public class State
        {
            public readonly Map<string, MemberState> Members;

            public static readonly State Empty = new State(Map.empty<string, MemberState>());

            public State(Map<string, MemberState> members)
            {
                Members = members;
            }

            public State SetMember(string nodeName, MemberState state) =>
                new State(Members.AddOrUpdate(nodeName, state));

            public State RemoveMember(string nodeName) =>
                new State(Members.Remove(nodeName));
        }

        public class MemberState
        {
            public readonly DateTime LastHeartbeat;

            public MemberState(
                DateTime lastHeartbeat
            )
            {
                LastHeartbeat = lastHeartbeat;
            }
        }

        /// <summary>
        /// Root Process setup
        /// </summary>
        public static State Setup() =>
            Heartbeat(State.Empty, ActorContext.Cluster);

        /// <summary>
        /// Root Process inbox
        /// </summary>
        public static State Inbox(State state, Msg msg)
        {
            switch (msg.Tag)
            {
                case MsgTag.Heartbeat:
                    state = Heartbeat(state, ActorContext.Cluster);
                    tellSelf(new Msg(MsgTag.Heartbeat), HeartbeatFreq + (random(1000)*milliseconds));
                    return state;
            }
            return state;
        }


        /// <summary>
        /// If this node is part of a cluster then it updates a shared map of 
        /// node-names to states.  This also downloads the latest map so the
        /// cluster state is known locally.
        /// </summary>
        /// <param name="state">Current state</param>
        /// <returns>Latest state from the cluster, or a map with just one item 'root'
        /// in it that represents this node.</returns>
        static State Heartbeat(State state, Option<ICluster> cluster) =>
            cluster.Map(
                c =>
                {
                    try
                    {
                        c.HashFieldAddOrUpdate(MembersKey, c.NodeName.Value, new MemberState(DateTime.UtcNow));
                        return new State(c.GetHashFields<MemberState>(MembersKey));
                    }
                    catch(Exception e)
                    {
                        logErr(e);
                        return HeartbeatLocal(state);
                    }
                })
            .IfNone(HeartbeatLocal(state));

        static State HeartbeatLocal(State state) =>
            state.SetMember("root", new MemberState(DateTime.UtcNow));

        static string GetNodeName(Option<ICluster> cluster) =>
            cluster.Map(c => c.NodeName.Value).IfNone("root");
    }
}
