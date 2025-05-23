import React, { useEffect } from "react";
import { createNativeStackNavigator } from "@react-navigation/native-stack";
import DrawerNavigation from "./DrawerNavigation";
import { useAuthStore } from "@/store";
import { useNavigation } from "@react-navigation/native";
import {
  UserRole,
  AuthNavigationProp,
  RootStackParamList,
  ROUTE_NAMES,
} from "@/constants";
import {
  AddNoteWorklogScreen,
  GraftedNoteScreen,
  GraftedPlantDetail,
  LoginScreen,
  NoteFormScreen,
  NotificationScreen,
  PestDetectionScreen,
  PlantDetailScreen,
  ReportResponseScreen,
  WorklogDetailScreen,
} from "@/screens";

const Stack = createNativeStackNavigator<RootStackParamList>();

export default function AppNavigation() {
  const { accessToken, refreshToken, roleId } = useAuthStore();
  const navigation = useNavigation<AuthNavigationProp>();

  // Đảm bảo điều hướng sau khi login thành công
  useEffect(() => {
    if (accessToken && refreshToken) {
      switch (roleId) {
        case UserRole.User.toString():
          navigation.navigate(ROUTE_NAMES.MAIN.DRAWER, {
            screen: ROUTE_NAMES.FARM.FARM_PICKER,
          });
          break;
        case UserRole.Admin.toString():
          navigation.navigate(ROUTE_NAMES.MAIN.DRAWER, {
            screen: ROUTE_NAMES.MAIN.MAIN_TABS,
          });
          break;
        case UserRole.Owner.toString():
          navigation.navigate(ROUTE_NAMES.MAIN.DRAWER, {
            screen: ROUTE_NAMES.MAIN.MAIN_TABS,
          });
          break;
        case UserRole.Manager.toString():
          navigation.navigate(ROUTE_NAMES.MAIN.DRAWER, {
            screen: ROUTE_NAMES.MAIN.MAIN_TABS,
          });
          break;
        case UserRole.Employee.toString():
          navigation.navigate(ROUTE_NAMES.MAIN.DRAWER, {
            screen: ROUTE_NAMES.MAIN.MAIN_TABS,
          });
          break;
        default:
          navigation.navigate(ROUTE_NAMES.AUTH.LOGIN);
          break;
      }
    }
  }, [accessToken, refreshToken, roleId, navigation]);

  return (
    <Stack.Navigator initialRouteName={ROUTE_NAMES.AUTH.LOGIN}>
      <Stack.Screen
        name={ROUTE_NAMES.AUTH.LOGIN}
        component={LoginScreen}
        options={{ headerShown: false, gestureEnabled: false }}
      />
      <Stack.Screen
        name={ROUTE_NAMES.MAIN.DRAWER}
        component={DrawerNavigation}
        options={{ headerShown: false, gestureEnabled: false }}
      />
      <Stack.Screen
        name={ROUTE_NAMES.PLANT.PLANT_DETAIL}
        component={PlantDetailScreen}
        options={{ headerShown: false }}
      />
      <Stack.Screen
        name={ROUTE_NAMES.PLANT.ADD_NOTE}
        component={NoteFormScreen}
        options={{ headerShown: false }}
      />
      <Stack.Screen
        name={ROUTE_NAMES.GRAFTED_PLANT.GRAFTED_PLANT_DETAIL}
        component={GraftedPlantDetail}
        options={{ headerShown: false }}
      />
      <Stack.Screen
        name={ROUTE_NAMES.GRAFTED_PLANT.ADD_NOTE}
        component={GraftedNoteScreen}
        options={{ headerShown: false }}
      />
      <Stack.Screen
        name={ROUTE_NAMES.WORKLOG.WORKLOG_DETAIL}
        component={WorklogDetailScreen}
        options={{ headerShown: false }}
      />
      <Stack.Screen
        name={ROUTE_NAMES.NOTIFICATION}
        component={NotificationScreen}
        options={{ headerShown: false }}
      />
      <Stack.Screen
        name={ROUTE_NAMES.WORKLOG.ADD_NOTE_WORKLOG}
        component={AddNoteWorklogScreen}
        options={{ headerShown: false }}
      />
      <Stack.Screen
        name={ROUTE_NAMES.PEST_DETECTION.PEST_DETECTION}
        component={PestDetectionScreen}
        options={{ headerShown: false }}
      />
      <Stack.Screen
        name={ROUTE_NAMES.EXPERT_RESPONSE}
        component={ReportResponseScreen}
        options={{ headerShown: false }}
      />
      {/* {!accessToken ? (
          <Stack.Screen
            name={ROUTE_NAMES.AUTH.LOGIN}
            component={LoginScreen}
            options={{ headerShown: false }}
          />
        ) : role === 'User' && !farmName ? (
          <Stack.Screen
            name={ROUTE_NAMES.FARM.FARM_PICKER}
            component={FarmPickerScreen}
            options={{ headerShown: false }}
          />
        ) : (
          <Stack.Screen
            name={ROUTE_NAMES.MAIN.MAIN_TABS}
            component={DrawerNavigation}
            options={{ headerShown: false }}
          />
        )} */}
    </Stack.Navigator>
  );
}
