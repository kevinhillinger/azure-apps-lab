using System;


public static void Run(object orderItem, out object orderDocument, TraceWriter log)
{
    log.Info($"Ticket Order received: {orderItem}");
    
    // Store in DocDB so we can query it for orders
    orderDocument = orderItem;
}
