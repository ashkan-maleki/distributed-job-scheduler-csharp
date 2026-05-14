# Distributed Job Scheduler (ML-Inspired)

A distributed systems project built to explore the core concepts behind modern orchestration and ML infrastructure systems such as Kubernetes, Spark, Ray, Airflow, and Celery.

The goal of this project is not to build a CRUD application, but to understand how distributed coordinators, workers, scheduling, lifecycle management, and fault handling work in real-world infrastructure systems.

---

# Current Status

Current implementation includes:

- Master / coordinator node
- Worker node
- Client application
- Distributed job lifecycle management
- Concurrent job state transitions
- Explicit execution protocol

This is an educational infrastructure project focused on:
- distributed coordination
- scheduling semantics
- concurrent state management
- worker lifecycle orchestration

---

# Architecture

```text
Client
   ↓
Master (Control Plane)
   ↓
Worker Nodes


Components
Master

The master node acts as the control plane of the system.

Responsibilities:

accept jobs from clients
assign jobs to workers
track distributed job state
validate lifecycle transitions
coordinate execution flow

Current implementation:

ASP.NET Core Minimal API
in-memory concurrent state store
atomic state transitions using ConcurrentDictionary
optimistic concurrency via TryUpdate
Worker

The worker node executes jobs assigned by the master.

Responsibilities:

request jobs
acknowledge execution start
simulate execution
report results back to master

Current implementation:

.NET Console Application
polling-based coordination
explicit execution lifecycle
Client

The client submits jobs to the scheduler.

Responsibilities:

create new jobs
send work requests to the master

Current implementation:

.NET Console Application
Distributed Job Lifecycle

The scheduler currently enforces the following job state machine:

Queued
   ↓
Assigned
   ↓
Running
   ↓
Completed / Failed
State Semantics
State	Meaning
Queued	Waiting for worker assignment
Assigned	Worker owns the job but execution has not started
Running	Worker acknowledged active execution
Completed	Execution finished successfully
Failed	Execution finished unsuccessfully
Current Protocol
1. Submit Job

Client submits a job:

POST /job
2. Assign Job

Worker polls master:

GET /job

Master:

finds queued job
atomically transitions state:
Queued -> Assigned
3. Start Execution

Worker acknowledges execution start:

POST /start

Master validates:

Assigned -> Running
4. Report Result

Worker reports execution result:

POST /result

Master validates:

Running -> Completed
Running -> Failed
Distributed Systems Concepts Explored

This project intentionally focuses on infrastructure and orchestration concerns rather than UI or CRUD development.

Current concepts explored:

control plane vs data plane
distributed coordination
optimistic concurrency
state machine design
atomic state transitions
worker orchestration
ownership semantics
scheduler lifecycle management
Current Limitations

The system is intentionally minimal and currently lacks:

persistent storage
retries
worker heartbeats
lease expiration
distributed consensus
worker identity
authentication
job timeout recovery
scheduling fairness
fault tolerance

These limitations are intentional and will be addressed incrementally as the system evolves.

Planned Improvements

Future work includes:

worker identity
lease-based scheduling
heartbeat mechanism
retry handling
timeout recovery
persistent state storage
lock-free scheduling
distributed coordination
horizontal scaling
observability and metrics
Technology Stack
Current (.NET Version)
.NET 10
ASP.NET Core Minimal API
ConcurrentDictionary
Console Applications
HTTP-based communication
Long-Term Goal

The long-term objective of this project is to build a serious systems engineering portfolio project demonstrating understanding of:

distributed systems
orchestration
scheduling
concurrency
ML infrastructure patterns
reliability engineering

This project is being implemented in:

C# / .NET
Go

as separate implementations to compare concurrency and systems programming approaches.