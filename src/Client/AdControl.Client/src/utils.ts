export function getScreensWord(count: number): string {
  const lastDigit = count % 10;
  const lastTwoDigits = count % 100;

  // Исключения для чисел 11-14
  if (lastTwoDigits >= 11 && lastTwoDigits <= 14) {
    return `${count} экранов`;
  }

  switch (lastDigit) {
    case 1:
      return `${count} экран`;
    case 2:
    case 3:
    case 4:
      return `${count} экрана`;
    default:
      return `${count} экранов`;
  }
}

export function joinResolutionData(width: string, height: string): string {
  return `${width}x${height}`;
}

export function getStatus(lastHeartBeat: number): string {
  return lastHeartBeat < 30 ? `подключено` : 'ошибка';
}

export function truncateString(str: string, maxLength: number, ellipsis = "...") {
  if (typeof str !== 'string') {
    throw new TypeError('Первый аргумент должен быть строкой');
  }

  if (str.length <= maxLength) {
    return str;
  }

  return str.slice(0, maxLength - ellipsis.length) + ellipsis;
}
