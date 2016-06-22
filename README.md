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
  .Do(createOrder) // task
  .Do(notifyUserOrderCreated) // task
    .And(notifyAccountManagerOrderCreated) // parallel task
    .And(notifyFulfillmentOrderCreated) // parallel task
  .WaitFor(orderPackaged)  // task. doesn't continue until state changes (monitored)
  .Condition(shippingApproved, onShipingApproved) // specification
    .WaitFor(orderShipped) // task
    .Do(notifyPackageShipped) // task
  
await workflow.Run();
```
