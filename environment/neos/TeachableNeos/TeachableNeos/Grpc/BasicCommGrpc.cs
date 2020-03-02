// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: basic_comm.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

public static partial class DataComm
{
  static readonly string __ServiceName = "DataComm";

  static readonly grpc::Marshaller<global::Empty> __Marshaller_Empty = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Empty.Parser.ParseFrom);
  static readonly grpc::Marshaller<global::NeosObservation> __Marshaller_NeosObservation = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::NeosObservation.Parser.ParseFrom);
  static readonly grpc::Marshaller<global::NeosAction> __Marshaller_NeosAction = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::NeosAction.Parser.ParseFrom);
  static readonly grpc::Marshaller<global::Response> __Marshaller_Response = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Response.Parser.ParseFrom);

  static readonly grpc::Method<global::Empty, global::NeosObservation> __Method_GetObs = new grpc::Method<global::Empty, global::NeosObservation>(
      grpc::MethodType.Unary,
      __ServiceName,
      "GetObs",
      __Marshaller_Empty,
      __Marshaller_NeosObservation);

  static readonly grpc::Method<global::NeosAction, global::Response> __Method_SendAct = new grpc::Method<global::NeosAction, global::Response>(
      grpc::MethodType.Unary,
      __ServiceName,
      "SendAct",
      __Marshaller_NeosAction,
      __Marshaller_Response);

  static readonly grpc::Method<global::Empty, global::Response> __Method_ResetAgent = new grpc::Method<global::Empty, global::Response>(
      grpc::MethodType.Unary,
      __ServiceName,
      "ResetAgent",
      __Marshaller_Empty,
      __Marshaller_Response);

  static readonly grpc::Method<global::Empty, global::NeosAction> __Method_GatherAct = new grpc::Method<global::Empty, global::NeosAction>(
      grpc::MethodType.Unary,
      __ServiceName,
      "GatherAct",
      __Marshaller_Empty,
      __Marshaller_NeosAction);

  /// <summary>Service descriptor</summary>
  public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
  {
    get { return global::BasicCommReflection.Descriptor.Services[0]; }
  }

  /// <summary>Base class for server-side implementations of DataComm</summary>
  public abstract partial class DataCommBase
  {
    /// <summary>
    /// (Method definitions not shown)
    /// </summary>
    /// <param name="request">The request received from the client.</param>
    /// <param name="context">The context of the server-side call handler being invoked.</param>
    /// <returns>The response to send back to the client (wrapped by a task).</returns>
    public virtual global::System.Threading.Tasks.Task<global::NeosObservation> GetObs(global::Empty request, grpc::ServerCallContext context)
    {
      throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
    }

    public virtual global::System.Threading.Tasks.Task<global::Response> SendAct(global::NeosAction request, grpc::ServerCallContext context)
    {
      throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
    }

    public virtual global::System.Threading.Tasks.Task<global::Response> ResetAgent(global::Empty request, grpc::ServerCallContext context)
    {
      throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
    }

    public virtual global::System.Threading.Tasks.Task<global::NeosAction> GatherAct(global::Empty request, grpc::ServerCallContext context)
    {
      throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
    }

  }

  /// <summary>Client for DataComm</summary>
  public partial class DataCommClient : grpc::ClientBase<DataCommClient>
  {
    /// <summary>Creates a new client for DataComm</summary>
    /// <param name="channel">The channel to use to make remote calls.</param>
    public DataCommClient(grpc::Channel channel) : base(channel)
    {
    }
    /// <summary>Creates a new client for DataComm that uses a custom <c>CallInvoker</c>.</summary>
    /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
    public DataCommClient(grpc::CallInvoker callInvoker) : base(callInvoker)
    {
    }
    /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
    protected DataCommClient() : base()
    {
    }
    /// <summary>Protected constructor to allow creation of configured clients.</summary>
    /// <param name="configuration">The client configuration.</param>
    protected DataCommClient(ClientBaseConfiguration configuration) : base(configuration)
    {
    }

    /// <summary>
    /// (Method definitions not shown)
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public virtual global::NeosObservation GetObs(global::Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
    {
      return GetObs(request, new grpc::CallOptions(headers, deadline, cancellationToken));
    }
    /// <summary>
    /// (Method definitions not shown)
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="options">The options for the call.</param>
    /// <returns>The response received from the server.</returns>
    public virtual global::NeosObservation GetObs(global::Empty request, grpc::CallOptions options)
    {
      return CallInvoker.BlockingUnaryCall(__Method_GetObs, null, options, request);
    }
    /// <summary>
    /// (Method definitions not shown)
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The call object.</returns>
    public virtual grpc::AsyncUnaryCall<global::NeosObservation> GetObsAsync(global::Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
    {
      return GetObsAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
    }
    /// <summary>
    /// (Method definitions not shown)
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="options">The options for the call.</param>
    /// <returns>The call object.</returns>
    public virtual grpc::AsyncUnaryCall<global::NeosObservation> GetObsAsync(global::Empty request, grpc::CallOptions options)
    {
      return CallInvoker.AsyncUnaryCall(__Method_GetObs, null, options, request);
    }
    public virtual global::Response SendAct(global::NeosAction request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
    {
      return SendAct(request, new grpc::CallOptions(headers, deadline, cancellationToken));
    }
    public virtual global::Response SendAct(global::NeosAction request, grpc::CallOptions options)
    {
      return CallInvoker.BlockingUnaryCall(__Method_SendAct, null, options, request);
    }
    public virtual grpc::AsyncUnaryCall<global::Response> SendActAsync(global::NeosAction request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
    {
      return SendActAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
    }
    public virtual grpc::AsyncUnaryCall<global::Response> SendActAsync(global::NeosAction request, grpc::CallOptions options)
    {
      return CallInvoker.AsyncUnaryCall(__Method_SendAct, null, options, request);
    }
    public virtual global::Response ResetAgent(global::Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
    {
      return ResetAgent(request, new grpc::CallOptions(headers, deadline, cancellationToken));
    }
    public virtual global::Response ResetAgent(global::Empty request, grpc::CallOptions options)
    {
      return CallInvoker.BlockingUnaryCall(__Method_ResetAgent, null, options, request);
    }
    public virtual grpc::AsyncUnaryCall<global::Response> ResetAgentAsync(global::Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
    {
      return ResetAgentAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
    }
    public virtual grpc::AsyncUnaryCall<global::Response> ResetAgentAsync(global::Empty request, grpc::CallOptions options)
    {
      return CallInvoker.AsyncUnaryCall(__Method_ResetAgent, null, options, request);
    }
    public virtual global::NeosAction GatherAct(global::Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
    {
      return GatherAct(request, new grpc::CallOptions(headers, deadline, cancellationToken));
    }
    public virtual global::NeosAction GatherAct(global::Empty request, grpc::CallOptions options)
    {
      return CallInvoker.BlockingUnaryCall(__Method_GatherAct, null, options, request);
    }
    public virtual grpc::AsyncUnaryCall<global::NeosAction> GatherActAsync(global::Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
    {
      return GatherActAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
    }
    public virtual grpc::AsyncUnaryCall<global::NeosAction> GatherActAsync(global::Empty request, grpc::CallOptions options)
    {
      return CallInvoker.AsyncUnaryCall(__Method_GatherAct, null, options, request);
    }
    /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
    protected override DataCommClient NewInstance(ClientBaseConfiguration configuration)
    {
      return new DataCommClient(configuration);
    }
  }

  /// <summary>Creates service definition that can be registered with a server</summary>
  /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
  public static grpc::ServerServiceDefinition BindService(DataCommBase serviceImpl)
  {
    return grpc::ServerServiceDefinition.CreateBuilder()
        .AddMethod(__Method_GetObs, serviceImpl.GetObs)
        .AddMethod(__Method_SendAct, serviceImpl.SendAct)
        .AddMethod(__Method_ResetAgent, serviceImpl.ResetAgent)
        .AddMethod(__Method_GatherAct, serviceImpl.GatherAct).Build();
  }

}
#endregion