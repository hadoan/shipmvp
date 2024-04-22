// https://stackoverflow.com/questions/56263980/get-key-of-an-enum-from-its-value-in-typescript
export function getEnumKeyByEnumValue(myEnum: any, enumValue: number | string): string {
  const keys = Object.keys(myEnum).filter((x) => myEnum[x] == enumValue);
  return keys.length > 0 ? keys[0] : "";
}
