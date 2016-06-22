# FluidFlow
Workflow manager with a fluid interface

##Sample Usage (Still in Development)

```csharp
// set up some specifications. Specifications allow you to create reusable processes for verifying a requirement

// each spec extends Specification<T>
var shippingInfoVerified = new ShippingApprovalsCompletedSpec(order);
var verificationOverridden = new ApprovalsOverrideSpec(order);
var shippingRequired = new ShippingRequiredSpec(order);

// specifications can be chained together
var shippingApproved = shippingRequired
                        .And(shippingInfoVerified)
                        .Or(verificationOverridden);

var packagedReceived = new ShippingCenterReceivedPackageSpec(order);
var packageLeftFacility = new PackageDepartedSpec(order);

// build a workflow

var workflow = new WorkflowActivity();

workflow
  .Do(createOrder) // activity
  .Do(notifyUserOrderCreated) // activity
    .Also(notifyAccountManagerOrderCreated) // parallel activity
    .Also(notifyFulfillmentOrderCreated) // parallel activity
  .WaitFor(orderPackaged)  // activity. doesn't continue until state changes (monitored)
  .If(shippingApproved) // specification. Creates a conditional if/then/else branch
    .WaitFor(orderShipped) // activity. doesn't continue until state changes (monitored)
    .If(orderDeparated) // specification
      .Do(notifyPackageShipped) // activity
    .Else()
      .FireAndForget(notifyAdmin) // activity
      .FireAndForget(notifyUserOrderFailed) // activity
    .EndIf()
  .Else()
    .FireAndForget(notifyUserOrderFailed) // activity
  .EndIf()
  .WaitFor(orderDelivered) // delayed activity
  .FireAndForget(notifyUserOrderDelivered); // activity
  
await workflow.Run();
```


- some implementations excluded for brevity
- each activity is a pre-implemented process. The obvious benefit is that these processes can be 
- reused in different workflows. 
- worflows are activities as well so you can have a step that is a completely separate workflow in itself
