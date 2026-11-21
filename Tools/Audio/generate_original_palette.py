"""Rebuild the original mono sound palette with the Python standard library."""
import array
import hashlib
import json
import math
import random
import wave
from pathlib import Path

SAMPLE_RATE = 48000
OUTPUT = Path(__file__).resolve().parents[2] / 'Assets/Marco/Audio/Original'


def noise_texture(duration, seed, kind):
    rng = random.Random(seed)
    samples = []
    low = 0.0
    slower = 0.0
    for index in range(round(duration * SAMPLE_RATE)):
        time = index / SAMPLE_RATE
        progress = time / duration
        noise = rng.uniform(-1, 1)
        low += 0.055 * (noise - low)
        slower += 0.007 * (noise - slower)
        envelope = math.sin(math.pi * progress) ** 1.7
        if kind == 'breath':
            pulse = 0.7 + 0.3 * math.sin(2 * math.pi * 0.6 * time - 0.8)
            grain = math.sin(2 * math.pi * 61 * time + 1.5 * math.sin(2 * math.pi * 2.2 * time))
            sample = envelope * (2.8 * (low - slower) * pulse + 0.028 * grain)
        else:
            scrape = (0.5 + 0.5 * math.sin(2 * math.pi * 5.7 * time)) ** 3
            sample = envelope * (low - slower) * (0.16 + scrape)
        samples.append(sample)
    return samples


def telephone_line():
    samples = []
    for index in range(round(2.6 * SAMPLE_RATE)):
        time = index / SAMPLE_RATE
        # Four deliberately separated pulses, softened at each boundary.
        phase = time % 0.65
        attack = min(1, phase / 0.025)
        release = max(0, min(1, (0.32 - phase) / 0.04))
        tone = math.sin(2 * math.pi * 425 * time) + 0.18 * math.sin(2 * math.pi * 850 * time)
        samples.append(tone * attack * release)
    return samples


def write_clip(name, samples, target_db):
    mean = sum(samples) / len(samples)
    centred = [sample - mean for sample in samples]
    peak = max(abs(sample) for sample in centred)
    gain = 10 ** (target_db / 20) / peak
    pcm = array.array('h', (round(sample * gain * 32767) for sample in centred))
    path = OUTPUT / (name + '.wav')
    with wave.open(str(path), 'wb') as audio:
        audio.setnchannels(1)
        audio.setsampwidth(2)
        audio.setframerate(SAMPLE_RATE)
        audio.writeframes(pcm.tobytes())
    assert max(abs(sample) for sample in pcm) < 32767
    assert abs(pcm[0]) < 16 and abs(pcm[-1]) < 16
    return {'file': path.name, 'seconds': len(pcm) / SAMPLE_RATE,
            'peak_dbfs': round(20 * math.log10(max(abs(sample) for sample in pcm) / 32767), 2),
            'rms_dbfs': round(20 * math.log10(math.sqrt(sum(sample * sample for sample in pcm) / len(pcm)) / 32767), 2),
            'sha256': hashlib.sha256(path.read_bytes()).hexdigest()}


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    definitions = [('creature_breath_01', 3.4, 3041, 'breath', -12),
                   ('creature_breath_02', 4.2, 3042, 'breath', -12),
                   ('creature_cloth_drag', 2.3, 3043, 'cloth', -14)]
    clips = [write_clip(name, noise_texture(duration, seed, kind), peak)
             for name, duration, seed, kind, peak in definitions]
    clips.append(write_clip('phone_unanswered', telephone_line(), -16))
    manifest = {'origin': 'Original procedural synthesis for Unspoken. No external recordings.',
                'generator': 'Tools/Audio/generate_original_palette.py', 'sample_rate': SAMPLE_RATE,
                'channels': 1, 'sample_bits': 16, 'clips': clips}
    (OUTPUT / 'SOURCES.json').write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(clips, indent=2))


if __name__ == '__main__':
    main()
