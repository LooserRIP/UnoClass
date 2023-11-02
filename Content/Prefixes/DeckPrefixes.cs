namespace UnoClass.Content.Prefixes {
    public class Bluffing : DeckPrefix {
        public Bluffing() : base( //copied this one STRAIGHT off clicker class, prefixes suck ass and someone did the work for me sooo
            damageMult: 1.1f, 
            rangeMult: 1.05f, 
            regenerationMult: 1f, 
            knockbackMult: 1f) {}
    }
    public class Messy : DeckPrefix {
        public Messy() : base(
            damageMult: 1f,
            rangeMult: 0.666f,
            regenerationMult: 1.25f,
            knockbackMult: 1f) { }
    }
    public class Flat : DeckPrefix {
        public Flat() : base(
            damageMult: 1f,
            rangeMult: 1f,
            regenerationMult: 1f,
            knockbackMult: 0.8f) { }
    }
    public class Folded : DeckPrefix {
        public Folded() : base(
            damageMult: 0.85f,
            rangeMult: 1f,
            regenerationMult: 1f,
            knockbackMult: 1f) { }
    }
    public class Shuffled : DeckPrefix {
        public Shuffled() : base(
            damageMult: 0.9f,
            rangeMult: 1.1f,
            regenerationMult: 1.1f,
            knockbackMult: 1f) { }
    }
    public class Big : DeckPrefix {
        public Big() : base(
            damageMult: 1f,
            rangeMult: 1.25f,
            regenerationMult: 1f,
            knockbackMult: 1f) { }
    }
    public class Organic : DeckPrefix {
        public Organic() : base(
            damageMult: 1f,
            rangeMult: 1.15f,
            regenerationMult: 1.15f,
            knockbackMult: 0.75f) { }
    }
    public class Shoddy : DeckPrefix {
        public Shoddy() : base(
            damageMult: 0.9f,
            rangeMult: 1f,
            regenerationMult: 1f,
            knockbackMult: 0.85f) { }
    }
}