﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConsolidationService.Infrastructure.Projections;

public sealed class ConsolidationProjection
{
    [BsonElement("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    public DateTime Date { get; set; }

    public string AccountId { get; set; }

    public decimal TotalDebits { get; set; }

    public decimal TotalCredits { get; set; }

    public decimal TotalAmount { get; set; }

    public IList<string> AppliedStreamIds { get; set; } = new List<string>();
}
