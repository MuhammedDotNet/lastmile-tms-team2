"use client";

import { useEffect, useRef, useState } from "react";
import type { Feature, FeatureCollection, LineString } from "geojson";
import type mapboxgl from "mapbox-gl";
import { getMapboxAccessToken, getZoneMapStyle } from "@/lib/mapbox/config";
import { cn } from "@/lib/utils";
import type { RoutePathPoint, RouteStop } from "@/types/routes";

const INITIAL_CENTER: [number, number] = [0, 18];
const INITIAL_ZOOM = 1.5;
const PATH_SOURCE_ID = "route-path";
const PATH_CASING_LAYER_ID = "route-path-casing";
const PATH_LAYER_ID = "route-path-line";

type MapboxModule = typeof import("mapbox-gl")["default"];

function buildPathFeature(path: RoutePathPoint[]): FeatureCollection<LineString> {
  const features: Feature<LineString>[] =
    path.length >= 2
      ? [
          {
            type: "Feature",
            properties: {},
            geometry: {
              type: "LineString",
              coordinates: path.map((point) => [point.longitude, point.latitude]),
            },
          },
        ]
      : [];

  return {
    type: "FeatureCollection",
    features,
  };
}

export function RouteMap({
  path,
  stops,
  className,
  emptyMessage = "Route preview will appear here once stops are planned.",
}: {
  path: RoutePathPoint[];
  stops: RouteStop[];
  className?: string;
  emptyMessage?: string;
}) {
  const accessToken = getMapboxAccessToken() ?? "";
  const mapStyle = getZoneMapStyle(accessToken);
  const containerRef = useRef<HTMLDivElement | null>(null);
  const mapRef = useRef<mapboxgl.Map | null>(null);
  const mapboxModuleRef = useRef<MapboxModule | null>(null);
  const markersRef = useRef<mapboxgl.Marker[]>([]);
  const [mapLoaded, setMapLoaded] = useState(false);
  const hasGeometry = path.length > 0 || stops.length > 0;

  useEffect(() => {
    let isCancelled = false;

    async function initializeMap() {
      if (!containerRef.current || mapRef.current) {
        return;
      }

      const mapboxModule = await import("mapbox-gl");
      if (isCancelled || !containerRef.current) {
        return;
      }

      const mapbox = mapboxModule.default;
      mapbox.accessToken = accessToken;
      mapboxModuleRef.current = mapbox;

      const map = new mapbox.Map({
        container: containerRef.current,
        style: mapStyle,
        center: INITIAL_CENTER,
        zoom: INITIAL_ZOOM,
        attributionControl: true,
        dragRotate: false,
        pitchWithRotate: false,
        projection: "mercator",
        renderWorldCopies: false,
      });

      map.addControl(
        new mapbox.NavigationControl({
          showCompass: false,
        }),
        "top-right",
      );
      map.addControl(
        new mapbox.ScaleControl({
          maxWidth: 120,
          unit: "metric",
        }),
        "bottom-right",
      );

      map.on("load", () => {
        if (isCancelled) {
          return;
        }

        map.addSource(PATH_SOURCE_ID, {
          type: "geojson",
          data: buildPathFeature(path),
        });

        map.addLayer({
          id: PATH_CASING_LAYER_ID,
          type: "line",
          source: PATH_SOURCE_ID,
          paint: {
            "line-color": "#f8fafc",
            "line-width": 7,
            "line-opacity": 0.95,
          },
          layout: {
            "line-join": "round",
            "line-cap": "round",
          },
        });

        map.addLayer({
          id: PATH_LAYER_ID,
          type: "line",
          source: PATH_SOURCE_ID,
          paint: {
            "line-color": "#0f172a",
            "line-width": 4,
            "line-opacity": 0.92,
          },
          layout: {
            "line-join": "round",
            "line-cap": "round",
          },
        });

        setMapLoaded(true);
      });

      mapRef.current = map;
    }

    void initializeMap();

    return () => {
      isCancelled = true;
      markersRef.current.forEach((marker) => marker.remove());
      markersRef.current = [];
      setMapLoaded(false);
      mapRef.current?.remove();
      mapRef.current = null;
    };
  }, [accessToken, mapStyle]);

  useEffect(() => {
    const map = mapRef.current;
    if (!mapLoaded || !map) {
      return;
    }

    const source = map.getSource(PATH_SOURCE_ID) as mapboxgl.GeoJSONSource | undefined;
    source?.setData(buildPathFeature(path));
  }, [mapLoaded, path]);

  useEffect(() => {
    const map = mapRef.current;
    const mapbox = mapboxModuleRef.current;
    if (!mapLoaded || !map || !mapbox) {
      return;
    }

    markersRef.current.forEach((marker) => marker.remove());
    markersRef.current = stops.map((stop) => {
      const element = document.createElement("button");
      element.type = "button";
      element.className =
        "flex size-8 items-center justify-center rounded-full border-2 border-white bg-slate-950 text-xs font-bold text-white shadow-lg";
      element.textContent = String(stop.sequence);

      return new mapbox.Marker({
        element,
        anchor: "center",
      })
        .setLngLat([stop.longitude, stop.latitude])
        .setPopup(
          new mapbox.Popup({
            closeButton: false,
            offset: 18,
          }).setHTML(
            `<div class="space-y-1">
              <div class="text-sm font-semibold">${stop.sequence}. ${stop.recipientLabel}</div>
              <div class="text-xs text-slate-600">${stop.addressLine}</div>
              <div class="text-xs text-slate-500">${stop.parcels.length} parcel${stop.parcels.length === 1 ? "" : "s"}</div>
            </div>`,
          ),
        )
        .addTo(map);
    });

    return () => {
      markersRef.current.forEach((marker) => marker.remove());
      markersRef.current = [];
    };
  }, [mapLoaded, stops]);

  useEffect(() => {
    const map = mapRef.current;
    const mapbox = mapboxModuleRef.current;
    if (!mapLoaded || !map || !mapbox || !hasGeometry) {
      return;
    }

    const bounds = new mapbox.LngLatBounds();
    for (const point of path) {
      bounds.extend([point.longitude, point.latitude]);
    }
    for (const stop of stops) {
      bounds.extend([stop.longitude, stop.latitude]);
    }

    if (bounds.isEmpty()) {
      return;
    }

    map.fitBounds(bounds, {
      padding: 56,
      duration: 600,
      maxZoom: 13,
    });
  }, [hasGeometry, mapLoaded, path, stops]);

  return (
    <div
      className={cn(
        "relative overflow-hidden rounded-[1.5rem] border border-border/60 bg-slate-100 shadow-[0_24px_64px_-36px_rgba(15,23,42,0.35)]",
        className,
      )}
    >
      <div ref={containerRef} className="h-[22rem] w-full sm:h-[28rem]" />
      {!mapLoaded ? (
        <div className="pointer-events-none absolute inset-0 flex items-center justify-center bg-slate-200/70 text-sm font-medium text-slate-700 backdrop-blur-sm">
          Loading route map...
        </div>
      ) : null}
      {mapLoaded && !hasGeometry ? (
        <div className="pointer-events-none absolute inset-0 flex items-center justify-center bg-slate-950/10 px-6 text-center text-sm font-medium text-slate-700">
          {emptyMessage}
        </div>
      ) : null}
    </div>
  );
}

export default RouteMap;
