mixin Serializable {
  Map<String, dynamic> serialize();
  void deserialize(Map<String, dynamic> from);
}
