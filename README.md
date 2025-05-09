# TokenHub

*Base URL*: `https://tokenhub.jabrayilov.az/api/v1/`

| Method  | Path    | 200 OK                                          | Other responses                                                                                                                                                             |
| ------- | ------- | ----------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **GET** | `Get`   | Returns the current **token** as a plain string | **503 Service Unavailable** – RFC 9457 ProblemDetails:<br>`{"title":"Token retrieval failed","detail":"The service could not retrieve a token at this time.","status":503}` |
| **GET** | `Reset` | Token existed ➜ reset completed, empty body     | **204 No Content** – no token existed, so nothing to reset                                                                                                                  |
