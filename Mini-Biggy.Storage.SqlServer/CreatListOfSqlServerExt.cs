namespace MiniBiggy {
    public static class CreatListOfSqlServerExt {
        public static IChooseOptions<T> SavingOnSqlServer<T>(this CreateListOf<T> list) where T : new() {
            return new CreateOnSqlServerBuilder<T>();
        }
    }
}