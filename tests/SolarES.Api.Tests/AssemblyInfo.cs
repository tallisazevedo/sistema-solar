// Hangfire expoe JobStorage.Current como estatico global; cada classe de teste que
// usa SolarESApiFactory reconfigura essa storage pra Hangfire.InMemory (T21). Rodar
// as classes em paralelo (padrao do xUnit p/ coleções diferentes) faz elas correrem
// por cima uma da outra nesse estado global. Serializar aqui e' mais simples e mais
// confiavel do que isolar o Hangfire por classe.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
