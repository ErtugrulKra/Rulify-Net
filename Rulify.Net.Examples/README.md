# Rulify.Net Examples

This project contains usage examples for the Rulify.Net library.

## Running

```bash
dotnet run --project Rulify.Net.Examples
```

## Contents

### 1. JSON Rules Example
Example of loading and using rules from JSON format.

### 2. Performance Test (1000 Booking Combinations)
Performance test for 1000 different accommodation combinations:
- Random booking parameters are generated
- Rules are evaluated for each combination
- Detailed performance metrics are shown:
  - Total execution time
  - Average per booking
  - Min/Max execution times
  - Bookings per second

### 3. Basic Usage Example
Basic rule usage examples.

### 4. Async Rules Example
Asynchronous rule usage.

### 5. Collection Evaluation Example
Batch data evaluation example.

### 6. Event Monitoring Example
Listening to rule evaluation events.

## Performance Test Features

The performance test includes the following scenarios:
- 1000 different booking combinations
- Random input parameters (stay duration, number of adults, number of children, etc.)
- A realistic travel reservation system simulation
- Detailed performance metrics

## JSON Rules Features

Supported rule types:
- **Hotel Price Calculation**: Weekday/weekend hotel price calculation
- **Child Discounts**: Age-based discount for children (using foreach)
- **Flight Discount**: Bulk purchase flight discount
- **Transfer Discount**: Transfer discount
- **Package Bonus**: Full package discount

